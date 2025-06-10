using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.Extensions;
using Aviationexam.DependencyUpdater.Nuget.Models;
using Microsoft.Extensions.Logging;
using NuGet.Packaging;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Aviationexam.DependencyUpdater.Nuget.Services;

public sealed class DependencyAnalyzer(
    NugetVersionFetcher nugetVersionFetcher,
    FutureVersionResolver futureVersionResolver,
    TargetFrameworksResolver targetFrameworksResolver,
    IgnoredDependenciesResolver ignoredDependenciesResolver,
    IgnoreResolverFactory ignoreResolverFactory,
    ILogger<DependencyAnalyzer> logger
)
{
    public async Task<DependencyAnalysisResult> AnalyzeDependenciesAsync(
        NugetUpdaterContext nugetUpdaterContext,
        IReadOnlyDictionary<NugetSource, NugetSourceRepository> sourceRepositories,
        IReadOnlyCollection<IgnoreEntry> ignoreEntries,
        IReadOnlyDictionary<string, PackageVersion> currentPackageVersions,
        CachingConfiguration cachingConfiguration,
        CancellationToken cancellationToken
    )
    {
        var ignoreResolver = ignoreResolverFactory.Create(ignoreEntries);

        // Resolve possible package versions
        var possiblePackageVersions = await ResolvePossiblePackageVersionsAsync(
            nugetUpdaterContext,
            sourceRepositories,
            ignoreResolver,
            cachingConfiguration,
            cancellationToken
        );

        var dependencyToUpdate = possiblePackageVersions.ToDictionary(
            x => x.Key,
            x => x.Value
        );

        // Initialize data structures for dependency analysis
        var packageFlags = new Dictionary<Package, EDependencyFlag>();
        var dependenciesToCheck = new Queue<(Package Package, IReadOnlyCollection<NugetTargetFramework> NugetTargetFrameworks)>();
        var dependenciesToRevisit = new Stack<(Package Package, IReadOnlyCollection<Package> Dependencies)>();

        // Process dependencies to update
        ProcessDependenciesToUpdate(
            ignoreResolver,
            currentPackageVersions,
            dependencyToUpdate,
            packageFlags,
            dependenciesToCheck
        );

        // Process dependencies to check
        await ProcessDependenciesToCheckAsync(
            ignoreResolver,
            currentPackageVersions,
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

    private async Task<IEnumerable<KeyValuePair<NugetDependency, IReadOnlyCollection<PossiblePackageVersion>>>> ResolvePossiblePackageVersionsAsync(
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
        await Parallel.ForEachAsync(dependencies, parallelOptions, async (dependencyPair, token) =>
        {
            var (dependency, sources) = dependencyPair;
            var dependencyName = dependency.NugetPackage.GetPackageName();
            var dependencyVersion = dependency.NugetPackage.GetVersion();

            try
            {
                var versions = await FetchDependencyVersionsAsync(
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
                    .Select(x => new PossiblePackageVersion(
                        x,
                        targetFrameworksResolver.GetCompatiblePackageDependencyGroups(
                            GetPreferredPackageSearchMetadataRegistration(x.OriginalReference),
                            dependency.TargetFrameworks
                        ).ToList()
                    ))
                    .Where(x => x.CompatiblePackageDependencyGroups.Count > 0)
                    .ToList();

                if (futureVersions.Count == 0)
                {
                    logger.LogDebug("The dependency {DependencyName} with version {Version} is up to date", dependencyName, dependencyVersion);
                    return;
                }

                results.Add(KeyValuePair.Create<NugetDependency, IReadOnlyCollection<PossiblePackageVersion>>(
                    dependency, futureVersions
                ));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing dependency {DependencyName} with version {Version}", dependencyName, dependencyVersion);
            }
        });

        return results.OrderBy(r => r.Key.NugetPackage.GetPackageName());
    }

    private async Task<IReadOnlyCollection<PackageVersion<PackageSearchMetadataRegistration>>> FetchDependencyVersionsAsync(
        NugetDependency dependency,
        IReadOnlyCollection<NugetSource> sources,
        IReadOnlyDictionary<NugetSource, NugetSourceRepository> sourceRepositories,
        CachingConfiguration cachingConfiguration,
        CancellationToken cancellationToken
    )
    {
        var versions = new List<PackageVersion<PackageSearchMetadataRegistration>>();
        var tasks = sources.Select<NugetSource, Task<IEnumerable<PackageVersion<PackageSearchMetadataRegistration>>>>(async nugetSource =>
        {
            if (sourceRepositories.TryGetValue(nugetSource, out var sourceRepository))
            {
                using var nugetCache = new SourceCacheContext();

                nugetCache.MaxAge = cachingConfiguration.MaxCacheAge;

                var rawPackageVersions = await nugetVersionFetcher.FetchPackageVersionsAsync(
                    sourceRepository.SourceRepository,
                    dependency,
                    nugetCache,
                    cancellationToken
                );
                var packageVersions = rawPackageVersions.Select(x => (Metadata: x, PackageVersion: x.MapToPackageVersion(), PackageSource: EPackageSource.Default));

                if (sourceRepository.FallbackSourceRepository is { } fallbackSourceRepository)
                {
                    var fallbackPackageVersions = await nugetVersionFetcher.FetchPackageVersionsAsync(
                        fallbackSourceRepository,
                        dependency,
                        nugetCache,
                        cancellationToken
                    );

                    packageVersions = packageVersions.Concat(fallbackPackageVersions.Select(x => (Metadata: x, PackageVersion: x.MapToPackageVersion(), PackageSource: EPackageSource.Fallback)));
                }

                return packageVersions.GroupBy(x => x.PackageVersion)
                    .Select(x => x.Key.MapToPackageVersion(x.ToDictionary(
                        d => d.PackageSource,
                        d => d.Metadata
                    )));
            }

            return [];
        });

        await foreach (var job in Task.WhenEach(tasks).WithCancellation(cancellationToken))
        {
            versions.AddRange(await job);
        }

        return versions;
    }

    private TOriginalReference GetPreferredPackageSearchMetadataRegistration<TOriginalReference>(
        IReadOnlyDictionary<EPackageSource, TOriginalReference> originalReference
    ) => originalReference
        .OrderBy(x => x.Key switch
        {
            EPackageSource.Default => 0,
            EPackageSource.Fallback => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(x.Key), x.Key, null),
        })
        .Select(x => x.Value)
        .First();

    private void ProcessDependenciesToUpdate(
        IgnoreResolver ignoreResolver,
        IReadOnlyDictionary<string, PackageVersion> currentPackageVersions,
        IReadOnlyDictionary<NugetDependency, IReadOnlyCollection<PossiblePackageVersion>> dependencyToUpdate,
        IDictionary<Package, EDependencyFlag> packageFlags,
        Queue<(Package Package, IReadOnlyCollection<NugetTargetFramework> NugetTargetFrameworks)> dependenciesToCheck
    )
    {
        foreach (var (dependency, possiblePackageVersions) in dependencyToUpdate)
        {
            foreach (var possiblePackageVersion in possiblePackageVersions)
            {
                foreach (var compatiblePackageDependencyGroup in possiblePackageVersion.CompatiblePackageDependencyGroups)
                {
                    _ = ProcessPackageDependencyGroup(
                        ignoreResolver,
                        currentPackageVersions,
                        packageFlags,
                        dependenciesToCheck,
                        compatiblePackageDependencyGroup,
                        dependency.TargetFrameworks
                    ).Count();
                }
            }
        }
    }

    private IEnumerable<Package> ProcessPackageDependencyGroup(
        IgnoreResolver ignoreResolver,
        IReadOnlyDictionary<string, PackageVersion> currentPackageVersions,
        IDictionary<Package, EDependencyFlag> packageFlags,
        Queue<(Package Package, IReadOnlyCollection<NugetTargetFramework> NugetTargetFrameworks)> dependenciesToCheck,
        PackageDependencyGroup compatiblePackageDependencyGroup,
        IReadOnlyCollection<NugetTargetFramework> targetFrameworks
    )
    {
        foreach (var packageDependency in compatiblePackageDependencyGroup.Packages)
        {
            if (packageDependency.VersionRange.MinVersion is not { } minVersion)
            {
                continue;
            }

            var dependentPackage = new Package(packageDependency.Id, minVersion.MapToPackageVersion());

            var isDependencyIgnored = ignoredDependenciesResolver.IsDependencyIgnored(
                packageDependency,
                ignoreResolver,
                currentPackageVersions
            );

            if (isDependencyIgnored)
            {
                packageFlags[dependentPackage] = EDependencyFlag.ContainsIgnoredDependency;
            }
            else if (packageFlags.TryAdd(dependentPackage, EDependencyFlag.Unknown))
            {
                dependenciesToCheck.Enqueue((dependentPackage, targetFrameworks));
            }

            yield return dependentPackage;
        }
    }

    private async Task ProcessDependenciesToCheckAsync(
        IgnoreResolver ignoreResolver,
        IReadOnlyDictionary<string, PackageVersion> currentPackageVersions,
        IDictionary<Package, EDependencyFlag> packageFlags,
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

            var nugetSources = context.GetSourcesForPackage(package.Name, logger);

            foreach (var nugetSource in nugetSources)
            {
                if (sourceRepositories.TryGetValue(nugetSource, out var sourceRepository))
                {
                    using var nugetCache = new SourceCacheContext();

                    nugetCache.MaxAge = cachingConfiguration.MaxCacheAge;

                    var packageMetadataMap = new Dictionary<EPackageSource, IPackageSearchMetadata>();

                    var defaultSourcePackageMetadata = await nugetVersionFetcher.FetchPackageMetadataAsync(
                        sourceRepository.SourceRepository,
                        package,
                        nugetCache,
                        cancellationToken
                    );

                    if (defaultSourcePackageMetadata is not null)
                    {
                        packageMetadataMap.Add(EPackageSource.Default, defaultSourcePackageMetadata);
                    }

                    if (sourceRepository.FallbackSourceRepository is { } fallbackSourceRepository)
                    {
                        var fallbackSourcePackageMetadata = await nugetVersionFetcher.FetchPackageMetadataAsync(
                            fallbackSourceRepository,
                            package,
                            nugetCache,
                            cancellationToken
                        );

                        if (fallbackSourcePackageMetadata is not null)
                        {
                            packageMetadataMap.Add(EPackageSource.Fallback, fallbackSourcePackageMetadata);
                        }
                    }

                    var packageMetadata = packageMetadataMap.MapToPackageVersion();

                    if (packageMetadata is null)
                    {
                        continue;
                    }

                    ProcessPackageMetadata(
                        ignoreResolver,
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
    }

    private void ProcessPackageMetadata(
        IgnoreResolver ignoreResolver,
        IReadOnlyDictionary<string, PackageVersion> currentPackageVersions,
        Package package,
        IReadOnlyCollection<NugetTargetFramework> targetFrameworks,
        PackageVersion<PackageSearchMetadataRegistration> packageVersion,
        IDictionary<Package, EDependencyFlag> packageFlags,
        Queue<(Package Package, IReadOnlyCollection<NugetTargetFramework> NugetTargetFrameworks)> dependenciesToCheck,
        Stack<(Package Package, IReadOnlyCollection<Package> Dependencies)> dependenciesToRevisit
    )
    {
        var packageSearchMetadataRegistration = GetPreferredPackageSearchMetadataRegistration(packageVersion.OriginalReference);
        var compatiblePackageDependencyGroups = targetFrameworksResolver.GetCompatiblePackageDependencyGroups(
            packageSearchMetadataRegistration,
            targetFrameworks
        );

        dependenciesToRevisit.Push((package, [
            .. compatiblePackageDependencyGroups.Aggregate(
                [],
                (IEnumerable<Package> acc, PackageDependencyGroup compatiblePackageDependencyGroup) => acc.Concat(ProcessPackageDependencyGroup(
                    ignoreResolver,
                    currentPackageVersions,
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
        IDictionary<Package, EDependencyFlag> packageFlags
    )
    {
        while (dependenciesToRevisit.TryPop(out var item))
        {
            var (package, dependencies) = item;

            var isIgnored = dependencies.Any(dependency =>
                packageFlags.TryGetValue(dependency, out var dependencyFlag)
                && dependencyFlag is EDependencyFlag.ContainsIgnoredDependency
            );

            packageFlags[package] = isIgnored
                ? EDependencyFlag.ContainsIgnoredDependency
                : EDependencyFlag.Valid;
        }
    }

}
