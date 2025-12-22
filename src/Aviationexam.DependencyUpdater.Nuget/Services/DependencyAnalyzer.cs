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
using ZLinq;

namespace Aviationexam.DependencyUpdater.Nuget.Services;

public sealed class DependencyAnalyzer(
    INugetVersionFetcher nugetVersionFetcher,
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
        IReadOnlyDictionary<string, IDictionary<string, PackageVersion>> currentPackageVersionsPerTargetFramework,
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

        var dependencyToUpdate = possiblePackageVersions.AsValueEnumerable().ToDictionary(
            x => x.Key,
            x => x.Value
        );

        // Initialize data structures for dependency analysis
        var packageFlags = new Dictionary<Package, IDictionary<NugetTargetFramework, EDependencyFlag>>();
        var dependenciesToCheck = new Queue<(Package Package, IReadOnlyCollection<NugetTargetFramework> NugetTargetFrameworks)>();
        var dependenciesToRevisit = new Stack<(Package Package, IReadOnlyCollection<Package> Dependencies)>();

        // Process dependencies to update
        ProcessDependenciesToUpdate(
            ignoreResolver,
            currentPackageVersionsPerTargetFramework,
            dependencyToUpdate,
            packageFlags,
            dependenciesToCheck
        );

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
                    .AsValueEnumerable()
                    .Select(x => new PossiblePackageVersion(
                        x,
                        targetFrameworksResolver.GetCompatiblePackageDependencyGroups(
                            GetPreferredDependencySets(x.DependencySets),
                            dependency.TargetFrameworks
                        ).AsValueEnumerable().ToList()
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

        return results.AsValueEnumerable().OrderBy(r => r.Key.NugetPackage.GetPackageName()).ToList();
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
        var tasks = sources.Select(async Task<IEnumerable<PackageVersion<PackageSearchMetadataRegistration>>> (nugetSource) =>
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
                    .Select(x => x.Key.MapToPackageVersion(x.AsValueEnumerable().ToDictionary(
                        d => d.PackageSource,
                        d => d.Metadata
                    )))
                    .ToList();
            }

            return [];
        });

        await foreach (var job in Task.WhenEach(tasks).WithCancellation(cancellationToken))
        {
            versions.AddRange(await job);
        }

        return versions;
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

    private void ProcessDependenciesToUpdate(
        IgnoreResolver ignoreResolver,
        IReadOnlyDictionary<string, IDictionary<string, PackageVersion>> currentPackageVersionsPerTargetFramework,
        IReadOnlyDictionary<NugetDependency, IReadOnlyCollection<PossiblePackageVersion>> dependencyToUpdate,
        IDictionary<Package, IDictionary<NugetTargetFramework, EDependencyFlag>> packageFlags,
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
                        currentPackageVersionsPerTargetFramework,
                        packageFlags,
                        dependenciesToCheck,
                        compatiblePackageDependencyGroup,
                        dependency.TargetFrameworks
                    ).AsValueEnumerable().Count();
                }
            }
        }
    }

    private IEnumerable<Package> ProcessPackageDependencyGroup(
        IgnoreResolver ignoreResolver,
        IReadOnlyDictionary<string, IDictionary<string, PackageVersion>> currentPackageVersionsPerTargetFramework,
        IDictionary<Package, IDictionary<NugetTargetFramework, EDependencyFlag>> packageFlags,
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

            // Initialize per-framework flags for this package if needed
            if (!packageFlags.TryGetValue(dependentPackage, out var frameworkFlags))
            {
                frameworkFlags = new Dictionary<NugetTargetFramework, EDependencyFlag>();
                packageFlags[dependentPackage] = frameworkFlags;
            }

            var shouldCheckDependency = false;
            foreach (var targetFramework in targetFrameworks)
            {
                if (frameworkFlags.ContainsKey(targetFramework))
                {
                    // Already processed for this target framework
                    continue;
                }

                // Check if this package is already at the correct version for this target framework
                if (
                    currentPackageVersionsPerTargetFramework.TryGetValue(packageDependency.Id, out var frameworkVersions)
                    && frameworkVersions.TryGetValue(targetFramework.TargetFramework, out var currentVersion)
                    && currentVersion == dependentPackage.Version
                )
                {
                    frameworkFlags[targetFramework] = EDependencyFlag.Valid;
                    continue;
                }

                var isDependencyIgnored = ignoredDependenciesResolver.IsDependencyIgnored(
                    packageDependency,
                    ignoreResolver,
                    currentPackageVersionsPerTargetFramework,
                    targetFramework
                );

                if (isDependencyIgnored)
                {
                    frameworkFlags[targetFramework] = EDependencyFlag.ContainsIgnoredDependency;
                }
                else
                {
                    frameworkFlags[targetFramework] = EDependencyFlag.Unknown;
                    shouldCheckDependency = true;
                }
            }

            if (shouldCheckDependency)
            {
                dependenciesToCheck.Enqueue((dependentPackage, targetFrameworks));
            }

            yield return dependentPackage;
        }
    }

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
    }

    private void ProcessPackageMetadata(
        IgnoreResolver ignoreResolver,
        IReadOnlyDictionary<string, IDictionary<string, PackageVersion>> currentPackageVersionsPerTargetFramework,
        Package package,
        IReadOnlyCollection<NugetTargetFramework> targetFrameworks,
        PackageVersion<PackageSearchMetadataRegistration> packageVersion,
        IDictionary<Package, IDictionary<NugetTargetFramework, EDependencyFlag>> packageFlags,
        Queue<(Package Package, IReadOnlyCollection<NugetTargetFramework> NugetTargetFrameworks)> dependenciesToCheck,
        Stack<(Package Package, IReadOnlyCollection<Package> Dependencies)> dependenciesToRevisit
    )
    {
        var compatiblePackageDependencyGroups = targetFrameworksResolver.GetCompatiblePackageDependencyGroups(
            GetPreferredDependencySets(packageVersion.DependencySets),
            targetFrameworks
        );

        dependenciesToRevisit.Push((package, [
            .. compatiblePackageDependencyGroups.AsValueEnumerable().Aggregate(
                [],
                (IEnumerable<Package> acc, PackageDependencyGroup compatiblePackageDependencyGroup) => acc.Concat(ProcessPackageDependencyGroup(
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
                if (flag != EDependencyFlag.Unknown)
                {
                    continue;
                }

                var isIgnored = dependencies.AsValueEnumerable().Any(dependency =>
                    packageFlags.TryGetValue(dependency, out var dependencyFrameworkFlags)
                    && dependencyFrameworkFlags.TryGetValue(targetFramework, out var dependencyFlag)
                    && dependencyFlag is EDependencyFlag.ContainsIgnoredDependency
                );

                frameworkFlags[targetFramework] = isIgnored
                    ? EDependencyFlag.ContainsIgnoredDependency
                    : EDependencyFlag.Valid;
            }
        }
    }
}
