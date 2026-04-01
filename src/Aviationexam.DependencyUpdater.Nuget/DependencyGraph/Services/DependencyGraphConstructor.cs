using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.Extensions;
using Aviationexam.DependencyUpdater.Nuget.Models;
using Aviationexam.DependencyUpdater.Nuget.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Aviationexam.DependencyUpdater.Nuget.DependencyGraph.Services;

public sealed class DependencyGraphConstructor(
    DependencyVersionsFetcher dependencyVersionsFetcher,
    ILogger<DependencyGraphConstructor> logger
)
{
    public async Task<DependencyGraph> ConstructGraphAsync(
        NugetUpdaterContext nugetUpdaterContext,
        IReadOnlyDictionary<NugetSource, NugetSourceRepository> sourceRepositories,
        CachingConfiguration cachingConfiguration,
        CancellationToken cancellationToken
    )
    {
        var graphBuilder = new DependencyGraphBuilder();
        var dependencies = nugetUpdaterContext.MapSourceToDependency(logger).ToList();

        var processingQueue = new Queue<(NugetDependency Dependency, IReadOnlyCollection<NugetSource> Sources)>();
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var (dependency, sources) in dependencies)
        {
            var packageName = dependency.NugetPackage.GetPackageName();
            var packageVersion = dependency.NugetPackage.GetVersion();

            if (packageVersion is null)
            {
                if (logger.IsEnabled(LogLevel.Warning))
                {
                    logger.LogWarning("Skipping dependency {PackageName} because package version is null", packageName);
                }

                continue;
            }

            graphBuilder.AddOrGetNode(packageName, packageVersion);
            processingQueue.Enqueue((dependency, sources));
        }

        while (processingQueue.TryDequeue(out var item))
        {
            var (dependency, sources) = item;
            var sourcePackageName = dependency.NugetPackage.GetPackageName();
            var sourcePackageVersion = dependency.NugetPackage.GetVersion();

            if (sourcePackageVersion is null)
            {
                continue;
            }

            if (!visited.Add(GetVisitedKey(sourcePackageName, sourcePackageVersion)))
            {
                continue;
            }

            var fetchedVersions = await dependencyVersionsFetcher.FetchDependencyVersionsAsync(
                dependency,
                sources,
                sourceRepositories,
                cachingConfiguration,
                cancellationToken
            );

            var currentVersion = fetchedVersions.FirstOrDefault(x => x == sourcePackageVersion);

            if (currentVersion is null)
            {
                graphBuilder.AddOrGetNode(sourcePackageName, sourcePackageVersion, isMetadataAvailable: false);

                if (logger.IsEnabled(LogLevel.Warning))
                {
                    logger.LogWarning(
                        "Unable to fetch metadata for package {PackageName} version {PackageVersion}",
                        sourcePackageName,
                        sourcePackageVersion
                    );
                }

                continue;
            }

            var sourceNode = graphBuilder.AddOrGetNode(sourcePackageName, sourcePackageVersion);
            var dependencySets = GetPreferredDependencySets(currentVersion.DependencySets);

            foreach (var dependencySet in dependencySets)
            {
                var targetFramework = new NugetTargetFramework(dependencySet.TargetFramework);

                foreach (var packageDependency in dependencySet.Packages)
                {
                    if (packageDependency.MinVersion is not { } minVersion)
                    {
                        continue;
                    }

                    var targetNode = graphBuilder.AddOrGetNode(packageDependency.Id, minVersion);
                    graphBuilder.AddEdge(sourceNode, targetNode, [targetFramework]);

                    var transitiveDependency = new NugetDependency(
                        dependency.NugetFile,
                        new NugetPackageVersion(packageDependency.Id, minVersion.GetSerializedVersion()),
                        [targetFramework]
                    );

                    var transitiveSources = nugetUpdaterContext.GetSourcesForPackage(packageDependency.Id, logger).ToList();
                    processingQueue.Enqueue((transitiveDependency, transitiveSources));
                }
            }
        }

        return graphBuilder.Build();
    }

    private static string GetVisitedKey(
        string packageName,
        PackageVersion packageVersion
    ) => $"{packageName}@{packageVersion.GetSerializedVersion()}";

    private static IReadOnlyCollection<DependencySet> GetPreferredDependencySets(
        IReadOnlyDictionary<EPackageSource, IReadOnlyCollection<DependencySet>> dependencySets
    )
    {
        if (dependencySets.TryGetValue(EPackageSource.Default, out var defaultDependencySets))
        {
            return defaultDependencySets;
        }

        if (dependencySets.TryGetValue(EPackageSource.Fallback, out var fallbackDependencySets))
        {
            return fallbackDependencySets;
        }

        return [];
    }
}
