using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.Extensions;
using Aviationexam.DependencyUpdater.Nuget.Helpers;
using Aviationexam.DependencyUpdater.Nuget.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        CachingConfiguration cachingConfiguration,
        CancellationToken cancellationToken
    )
    {
        var ignoreResolver = ignoreResolverFactory.Create(ignoreEntries);

        // Resolve possible package versions
        var possiblePackageVersionsResult = await ResolvePossiblePackageVersionsAsync(
            nugetUpdaterContext,
            sourceRepositories,
            ignoreResolver,
            cachingConfiguration,
            cancellationToken
        );

        // Process dependencies to update
        var processingResult = dependencyUpdateProcessor.ProcessDependenciesToUpdate(
            ignoreResolver,
            possiblePackageVersionsResult.CurrentPackageVersions,
            possiblePackageVersionsResult.DependencyToUpdate
        );

        var packageFlags = processingResult.PackageFlags;
        var dependenciesToRevisit = new Stack<(Package Package, NugetPackageCondition Condition, IReadOnlyCollection<Package> Dependencies)>();

        // Process dependencies to check
        await ProcessDependenciesToCheckAsync(
            ignoreResolver,
            possiblePackageVersionsResult.CurrentPackageVersions,
            packageFlags,
            processingResult.DependenciesToCheck,
            sourceRepositories,
            nugetUpdaterContext,
            dependenciesToRevisit,
            cachingConfiguration,
            cancellationToken
        );

        // Process dependencies to revisit
        ProcessDependenciesToRevisit(dependenciesToRevisit, packageFlags);

        return new DependencyAnalysisResult(
            possiblePackageVersionsResult.DependencyToUpdate,
            possiblePackageVersionsResult.CurrentPackageVersions,
            packageFlags
        );
    }

    private async Task<ResolvePossiblePackageVersionsResult> ResolvePossiblePackageVersionsAsync(
        NugetUpdaterContext nugetUpdaterContext,
        IReadOnlyDictionary<NugetSource, NugetSourceRepository> sourceRepositories,
        IgnoreResolver ignoreResolver,
        CachingConfiguration cachingConfiguration,
        CancellationToken cancellationToken
    )
    {
        var dependencies = nugetUpdaterContext.MapSourceToDependency(logger);
        var results = new ConcurrentBag<KeyValuePair<UpdateCandidate, IReadOnlyCollection<PossiblePackageVersion>>>();
        var currentVersions = new ConcurrentBag<KeyValuePair<string, IDictionary<NugetPackageCondition, IDictionary<NugetTargetFrameworkGroup, PackageVersion>>>>();

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
            var dependencyCondition = dependency.NugetPackage.GetCondition();

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
                        GetPreferredDependencySets(x.DependencySets, dependency.TargetFrameworks)
                    ))
                    .Where(x => x.CompatibleDependencySets.Count > 0)
                    .ToList();

                var currentVersion = versions.AsValueEnumerable().SingleOrDefault(x => dependencyVersion is not null && dependencyVersion == x);

                if (currentVersion?.DependencySets is { } dependencySets)
                {
                    var nugetTargetFrameworkGroup = new NugetTargetFrameworkGroup(GetPreferredDependencySets(
                            dependencySets,
                            dependency.TargetFrameworks
                        )
                        .AsValueEnumerable()
                        .Select(x => new NugetTargetFramework(x.TargetFramework))
                        .ToList()
                    );

                    currentVersions.Add(KeyValuePair.Create<string, IDictionary<NugetPackageCondition, IDictionary<NugetTargetFrameworkGroup, PackageVersion>>>(
                        dependencyName,
                        new Dictionary<NugetPackageCondition, IDictionary<NugetTargetFrameworkGroup, PackageVersion>>
                        {
                            [dependencyCondition] = new Dictionary<NugetTargetFrameworkGroup, PackageVersion>
                            {
                                [nugetTargetFrameworkGroup] = dependencyVersion!,
                            },
                        }
                    ));
                }

                if (futureVersions.Count == 0)
                {
                    if (logger.IsEnabled(LogLevel.Debug))
                    {
                        logger.LogDebug("The dependency {DependencyName} with version {Version} is up to date", dependencyName, dependencyVersion);
                    }

                    return;
                }

                results.Add(KeyValuePair.Create<UpdateCandidate, IReadOnlyCollection<PossiblePackageVersion>>(
                    new UpdateCandidate(
                        dependency,
                        currentVersion,
                        currentVersion?.DependencySets is null
                            ? null
                            : new NugetTargetFrameworkGroup(GetPreferredDependencySets(currentVersion.DependencySets, dependency.TargetFrameworks)
                                .AsValueEnumerable()
                                .Select(x => new NugetTargetFramework(x.TargetFramework))
                                .ToList()
                            )
                    ),
                    futureVersions
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

        return new ResolvePossiblePackageVersionsResult(
            results.AsValueEnumerable()
                .OrderBy(r => r.Key.NugetDependency.NugetPackage.GetPackageName())
                .ToDictionary(),
            CurrentPackageVersions.FromConcurrentBag(currentVersions)
        );
    }

    private IReadOnlyCollection<DependencySet> GetPreferredDependencySets(
        IEnumerable<KeyValuePair<EPackageSource, IReadOnlyCollection<DependencySet>>> dependencySets,
        IReadOnlyCollection<NugetTargetFramework> targetFrameworks
    ) => dependencySets
        .AsValueEnumerable()
        .OrderBy(x => x.Key switch
        {
            EPackageSource.Default => 0,
            EPackageSource.Fallback => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(x.Key), x.Key, null),
        })
        .Select(x => targetFrameworksResolver.GetCompatibleDependencySets(
            x.Value,
            targetFrameworks
        ).AsValueEnumerable().ToList())
        .FirstOrDefault() ?? [];


    private async Task ProcessDependenciesToCheckAsync(
        IgnoreResolver ignoreResolver,
        CurrentPackageVersions currentPackageVersions,
        IDictionary<Package, IDictionary<NugetTargetFramework, EDependencyFlag>> packageFlags,
        Queue<(Package Package, NugetPackageCondition Condition, IReadOnlyCollection<NugetTargetFramework> NugetTargetFrameworks)> dependenciesToCheck,
        IReadOnlyDictionary<NugetSource, NugetSourceRepository> sourceRepositories,
        NugetUpdaterContext context,
        Stack<(Package Package, NugetPackageCondition Condition, IReadOnlyCollection<Package> Dependencies)> dependenciesToRevisit,
        CachingConfiguration cachingConfiguration,
        CancellationToken cancellationToken
    )
    {
        while (dependenciesToCheck.TryDequeue(out var item))
        {
            var (package, condition, targetFrameworks) = item;

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
                    condition,
                    currentPackageVersions,
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
        NugetPackageCondition condition,
        CurrentPackageVersions currentPackageVersions,
        Package package,
        IReadOnlyCollection<NugetTargetFramework> targetFrameworks,
        PackageVersionWithDependencySets packageVersion,
        IDictionary<Package, IDictionary<NugetTargetFramework, EDependencyFlag>> packageFlags,
        Queue<(Package Package, NugetPackageCondition Condition, IReadOnlyCollection<NugetTargetFramework> NugetTargetFrameworks)> dependenciesToCheck,
        Stack<(Package Package, NugetPackageCondition Condition, IReadOnlyCollection<Package> Dependencies)> dependenciesToRevisit
    )
    {
        var compatiblePackageDependencyGroups = GetPreferredDependencySets(
            packageVersion.DependencySets, targetFrameworks
        );

        dependenciesToRevisit.Push((package, condition, [
            .. compatiblePackageDependencyGroups
                .AsValueEnumerable()
                .SelectMany(compatiblePackageDependencyGroup => dependencyUpdateProcessor.ProcessDependencySet(
                    ignoreResolver,
                    condition,
                    currentPackageVersions,
                    packageFlags,
                    dependenciesToCheck,
                    compatiblePackageDependencyGroup
                )),
        ]));
    }

    private void ProcessDependenciesToRevisit(
        Stack<(Package Package, NugetPackageCondition Condition, IReadOnlyCollection<Package> Dependencies)> dependenciesToRevisit,
        IDictionary<Package, IDictionary<NugetTargetFramework, EDependencyFlag>> packageFlags
    )
    {
        while (dependenciesToRevisit.TryPop(out var item))
        {
            var (package, condition, dependencies) = item;

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
