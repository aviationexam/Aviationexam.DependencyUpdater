using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.Extensions;
using Aviationexam.DependencyUpdater.Nuget.Helpers;
using Aviationexam.DependencyUpdater.Nuget.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ZLinq;

namespace Aviationexam.DependencyUpdater.Nuget.Services;

public sealed class DependencyAnalyzer(
    IDependencyVersionsFetcher dependencyVersionsFetcher,
    FutureVersionResolver futureVersionResolver,
    TargetFrameworksResolver targetFrameworksResolver,
    IgnoreResolverFactory ignoreResolverFactory,
    DependencyUpdateProcessor dependencyUpdateProcessor,
    ILogger<DependencyAnalyzer> logger
)
{
    public async Task<DependencyAnalysisResult> AnalyzeDependenciesAsync(
        NugetUpdaterContext nugetUpdaterContext,
        IReadOnlyDictionary<NugetSource, NugetSourceRepository> sourceRepositories,
        IReadOnlyCollection<IgnoreEntry> ignoreEntries,
        IReadOnlyDictionary<string, IDictionary<string, PackageVersion>> currentPackageVersionsPerTargetFramework,
        CachingConfiguration cachingConfiguration,
        CancellationToken cancellationToken
    )
    {
        var ignoreResolver = ignoreResolverFactory.Create(ignoreEntries);

        // Resolve possible package versions
        var dependencyToUpdate = await ResolvePossiblePackageVersionsAsync(
            nugetUpdaterContext,
            sourceRepositories,
            ignoreResolver,
            cachingConfiguration,
            cancellationToken
        );

        // Process dependencies to update
        var processingResult = dependencyUpdateProcessor.ProcessDependenciesToUpdate(
            ignoreResolver,
            currentPackageVersionsPerTargetFramework,
            dependencyToUpdate
        );

        var packageFlags = processingResult.PackageFlags;
        var dependenciesToCheck = processingResult.DependenciesToCheck;
        var dependenciesToRevisit = new Stack<(Package Package, IReadOnlyCollection<Package> Dependencies)>();

        // Process dependencies to check
        await ProcessDependenciesToCheckAsync(
            ignoreResolver,
            currentPackageVersionsPerTargetFramework,
            packageFlags,
            dependenciesToCheck,
            sourceRepositories,
            nugetUpdaterContext,
            dependenciesToRevisit,
            cachingConfiguration,
            cancellationToken
        );

        // Process dependencies to revisit
        ProcessDependenciesToRevisit(dependenciesToRevisit, packageFlags);

        return new DependencyAnalysisResult(dependencyToUpdate, packageFlags);
    }

    private async Task<IReadOnlyDictionary<NugetDependency, IReadOnlyCollection<PossiblePackageVersion>>> ResolvePossiblePackageVersionsAsync(
        NugetUpdaterContext nugetUpdaterContext,
        IReadOnlyDictionary<NugetSource, NugetSourceRepository> sourceRepositories,
        IgnoreResolver ignoreResolver,
        CachingConfiguration cachingConfiguration,
        CancellationToken cancellationToken
    )
    {
        var dependencies = nugetUpdaterContext.MapSourceToDependency(logger);
        var results = new ConcurrentBag<KeyValuePair<NugetDependency, IReadOnlyCollection<PossiblePackageVersion>>>();

        // Limit parallelism to avoid overwhelming system resources
        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount / 2),
            CancellationToken = cancellationToken,
        };

        // Process dependencies in parallel
        await ProcessCollectionHelper.ForEachAsync(dependencies, parallelOptions, async (dependencyPair, token) =>
        {
            var (dependency, sources) = dependencyPair;
            var dependencyName = dependency.NugetPackage.GetPackageName();
            var dependencyVersion = dependency.NugetPackage.GetVersion();

            try
            {
                var versions = await dependencyVersionsFetcher.FetchDependencyVersionsAsync(
                    dependency,
                    sources,
                    sourceRepositories,
                    cachingConfiguration,
                    token
                );

                var futureVersions = futureVersionResolver.ResolveFutureVersion(
                        dependencyName,
                        dependencyVersion,
                        versions,
                        ignoreResolver
                    )
                    .AsValueEnumerable()
                    .Select(x => new PossiblePackageVersion(
                        x,
                        GetPreferredDependencySets(x.DependencySets),
                        dependencyVersion is not null && dependencyVersion == x
                    ))
                    .Where(x => x.CompatibleDependencySets.Count > 0)
                    .ToList();

                if (futureVersions.Count == 0)
                {
                    if (logger.IsEnabled(LogLevel.Debug))
                    {
                        logger.LogDebug("The dependency {DependencyName} with version {Version} is up to date", dependencyName, dependencyVersion);
                    }

                    return;
                }

                results.Add(KeyValuePair.Create<NugetDependency, IReadOnlyCollection<PossiblePackageVersion>>(
                    dependency, futureVersions
                ));
            }
            catch (Exception ex)
            {
                if (logger.IsEnabled(LogLevel.Error))
                {
                    logger.LogError(ex, "Error processing dependency {DependencyName} with version {Version}", dependencyName, dependencyVersion);
                }
            }
        });

        return results.AsValueEnumerable()
            .OrderBy(r => r.Key.NugetPackage.GetPackageName())
            .ToDictionary();
    }


    private IReadOnlyCollection<DependencySet> GetPreferredDependencySets(
        IReadOnlyDictionary<EPackageSource, IReadOnlyCollection<DependencySet>> dependencySets
    ) => dependencySets
        .AsValueEnumerable()
        .OrderBy(x => x.Key switch
        {
            EPackageSource.Default => 0,
            EPackageSource.Fallback => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(x.Key), x.Key, null),
        })
        .Select(x => x.Value)
        .FirstOrDefault() ?? [];


    private async Task ProcessDependenciesToCheckAsync(
        IgnoreResolver ignoreResolver,
        IReadOnlyDictionary<string, IDictionary<string, PackageVersion>> currentPackageVersionsPerTargetFramework,
        IDictionary<Package, IDictionary<NugetTargetFramework, EDependencyFlag>> packageFlags,
        Queue<(Package Package, IReadOnlyCollection<NugetTargetFramework> NugetTargetFrameworks)> dependenciesToCheck,
        IReadOnlyDictionary<NugetSource, NugetSourceRepository> sourceRepositories,
        NugetUpdaterContext context,
        Stack<(Package Package, IReadOnlyCollection<Package> Dependencies)> dependenciesToRevisit,
        CachingConfiguration cachingConfiguration,
        CancellationToken cancellationToken
    )
    {
        while (dependenciesToCheck.TryDequeue(out var item))
        {
            var (package, targetFrameworks) = item;

            var packageMetadata = await dependencyVersionsFetcher.FetchPackageMetadataAsync(
                package,
                [.. context.GetSourcesForPackage(package.Name, logger)],
                sourceRepositories,
                cachingConfiguration,
                cancellationToken
            );

            if (packageMetadata is not null)
            {
                ProcessPackageMetadata(
                    ignoreResolver,
                    currentPackageVersionsPerTargetFramework,
                    package,
                    targetFrameworks,
                    packageMetadata,
                    packageFlags,
                    dependenciesToCheck,
                    dependenciesToRevisit
                );
            }
        }
    }

    private void ProcessPackageMetadata(
        IgnoreResolver ignoreResolver,
        IReadOnlyDictionary<string, IDictionary<string, PackageVersion>> currentPackageVersionsPerTargetFramework,
        Package package,
        IReadOnlyCollection<NugetTargetFramework> targetFrameworks,
        PackageVersionWithDependencySets packageVersion,
        IDictionary<Package, IDictionary<NugetTargetFramework, EDependencyFlag>> packageFlags,
        Queue<(Package Package, IReadOnlyCollection<NugetTargetFramework> NugetTargetFrameworks)> dependenciesToCheck,
        Stack<(Package Package, IReadOnlyCollection<Package> Dependencies)> dependenciesToRevisit
    )
    {
        var compatiblePackageDependencyGroups = targetFrameworksResolver.GetCompatibleDependencySets(
            GetPreferredDependencySets(packageVersion.DependencySets),
            targetFrameworks
        );

        dependenciesToRevisit.Push((package, [
            .. compatiblePackageDependencyGroups.AsValueEnumerable().Aggregate(
                [],
                (IEnumerable<Package> acc, DependencySet compatiblePackageDependencyGroup) => acc.Concat(dependencyUpdateProcessor.ProcessDependencySet(
                    ignoreResolver,
                    currentPackageVersionsPerTargetFramework,
                    packageFlags,
                    dependenciesToCheck,
                    compatiblePackageDependencyGroup,
                    targetFrameworks
                ))
            ),
        ]));
    }

    private void ProcessDependenciesToRevisit(
        Stack<(Package Package, IReadOnlyCollection<Package> Dependencies)> dependenciesToRevisit,
        IDictionary<Package, IDictionary<NugetTargetFramework, EDependencyFlag>> packageFlags
    )
    {
        while (dependenciesToRevisit.TryPop(out var item))
        {
            var (package, dependencies) = item;

            if (!packageFlags.TryGetValue(package, out var frameworkFlags))
            {
                continue;
            }

            // Update flags for each target framework
            foreach (var (targetFramework, flag) in frameworkFlags)
            {
                if (flag is not EDependencyFlag.Unknown)
                {
                    continue;
                }

                var isIgnored = dependencies.AsValueEnumerable().Any(dependency =>
                    packageFlags.TryGetValue(dependency, out var dependencyFrameworkFlags)
                    && dependencyFrameworkFlags.TryGetCompatibleFramework(targetFramework, out var dependencyFlag)
                    && dependencyFlag is EDependencyFlag.ContainsIgnoredDependency
                );

                frameworkFlags[targetFramework] = isIgnored
                    ? EDependencyFlag.ContainsIgnoredDependency
                    : EDependencyFlag.Valid;
            }
        }
    }
}
