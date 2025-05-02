using Aviationexam.DependencyUpdater.Common;
using Microsoft.Extensions.Logging;
using NuGet.Packaging;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Aviationexam.DependencyUpdater.Nuget;

public sealed class NugetUpdater(
    NugetFinder nugetFinder,
    NugetConfigParser nugetConfigParser,
    NugetDirectoryPackagesPropsParser nugetDirectoryPackagesPropsParser,
    NugetCsprojParser nugetCsprojParser,
    NugetVersionFetcherFactory nugetVersionFetcherFactory,
    NugetVersionFetcher nugetVersionFetcher,
    FutureVersionResolver futureVersionResolver,
    IgnoreResolverFactory ignoreResolverFactory,
    TargetFrameworksResolver targetFrameworksResolver,
    IgnoredDependenciesResolver ignoredDependenciesResolver,
    ILogger<NugetUpdater> logger
)
{
    public async Task ProcessUpdatesAsync(
        string directoryPath,
        IReadOnlyCollection<NugetFeedAuthentication> nugetFeedAuthentications,
        IReadOnlyCollection<NugetTargetFramework> targetFrameworks,
        IReadOnlyCollection<IgnoreEntry> ignoreEntries,
        IReadOnlyCollection<GroupEntry> groupEntries,
        CancellationToken cancellationToken = default
    )
    {
        var nugetUpdaterContext = CreateContext(
            directoryPath,
            targetFrameworks
        );

        var currentPackageVersions = nugetUpdaterContext.Dependencies
            .Select(x => x.NugetPackage)
            .Select(x => (PackageName: x.GetPackageName(), Version: x.GetVersion()))
            .Where(x => x.Version is not null)
            .GroupBy(x => x.PackageName)
            .ToDictionary(x => x.Key, x => x.OrderBy(y => y.Version).First().Version!);

        var sourceRepositories = nugetUpdaterContext.NugetConfigurations
            .GroupBy(x => x.Source)
            .Select(x => x.First())
            .ToDictionary(
                x => x,
                x => nugetVersionFetcherFactory.CreateSourceRepository(x, nugetFeedAuthentications)
            );

        var ignoreResolver = ignoreResolverFactory.Create(ignoreEntries);

        var dependencyToUpdate = await ResolvePossiblePackageVersions(
            nugetUpdaterContext,
            currentPackageVersions,
            sourceRepositories,
            ignoreResolver,
            cancellationToken
        ).ToDictionaryAsync(
            x => x.dependency,
            x => x.futureVersions,
            cancellationToken
        );

        var a = dependencyToUpdate;
    }

    public NugetUpdaterContext CreateContext(
        string directoryPath,
        IReadOnlyCollection<NugetTargetFramework> targetFrameworks
    )
    {
        var nugetConfigurations = nugetFinder.GetNugetConfig(directoryPath)
            .SelectMany(nugetConfigParser.Parse)
            .ToList();

        var dependencies = nugetFinder.GetDirectoryPackagesPropsFiles(directoryPath)
            .SelectMany(x => nugetDirectoryPackagesPropsParser.Parse(x, targetFrameworks))
            .Concat(
                nugetFinder.GetAllCsprojFiles(directoryPath)
                    .SelectMany(x => nugetCsprojParser.Parse(x))
                    .Where(x => x.NugetPackage is NugetPackageReference { VersionRange: not null })
            )
            .ToList();

        return new NugetUpdaterContext(
            nugetConfigurations,
            dependencies
        );
    }

    private sealed record PossiblePackageVersion(
        PackageVersion<PackageSearchMetadataRegistration> PackageVersion,
        IReadOnlyCollection<PackageDependencyGroup> CompatiblePackageDependencyGroups
    );

    private async IAsyncEnumerable<(NugetDependency dependency, IReadOnlyCollection<PossiblePackageVersion> futureVersions)> ResolvePossiblePackageVersions(
        NugetUpdaterContext nugetUpdaterContext,
        IReadOnlyDictionary<string, PackageVersion> currentPackageVersions,
        IReadOnlyDictionary<NugetSource, SourceRepository> sourceRepositories,
        IgnoreResolver ignoreResolver,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        var dependencies = nugetUpdaterContext.MapSourceToDependency(logger);
        foreach (var (dependency, sources) in dependencies)
        {
            var versions = await FetchDependencyVersionsAsync(
                dependency,
                sources,
                sourceRepositories,
                cancellationToken
            );

            var dependencyName = dependency.NugetPackage.GetPackageName();
            var dependencyVersion = dependency.NugetPackage.GetVersion();

            var futureVersions = futureVersionResolver.ResolveFutureVersion(
                    dependencyName,
                    dependencyVersion,
                    versions.Select(NugetMapper.MapToPackageVersion),
                    ignoreResolver
                )
                .Select(x => new PossiblePackageVersion(
                    x,
                    targetFrameworksResolver.GetCompatiblePackageDependencyGroups(
                        x.OriginalReference,
                        dependency.TargetFrameworks
                    ).ToList()
                ))
                .Select(x => x with
                {
                    CompatiblePackageDependencyGroups = ignoredDependenciesResolver.FilterDependencyGroupsRequiringIgnoredPackages(
                        x.CompatiblePackageDependencyGroups,
                        ignoreResolver,
                        currentPackageVersions
                    ).ToList(),
                })
                .Where(x => x.CompatiblePackageDependencyGroups.Count > 0)
                .ToList();

            if (futureVersions.Count == 0)
            {
                logger.LogDebug("The dependency {DependencyName} with version {Version} is up to date", dependencyName, dependencyVersion);
                continue;
            }

            yield return (dependency, futureVersions);
        }
    }

    private async Task<IReadOnlyCollection<IPackageSearchMetadata>> FetchDependencyVersionsAsync(
        NugetDependency dependency,
        IReadOnlyCollection<NugetSource> sources,
        IReadOnlyDictionary<NugetSource, SourceRepository> sourceRepositories,
        CancellationToken cancellationToken
    )
    {
        var versions = new List<IPackageSearchMetadata>();
        var tasks = sources.Select(async nugetSource =>
        {
            if (sourceRepositories.TryGetValue(nugetSource, out var sourceRepository))
            {
                using var nugetCache = new SourceCacheContext();

                var packageVersions = await nugetVersionFetcher.FetchPackageVersionsAsync(
                    sourceRepository,
                    dependency,
                    nugetCache,
                    cancellationToken
                );

                return packageVersions.ToList();
            }

            return [];
        });

        await foreach (var job in Task.WhenEach(tasks).WithCancellation(cancellationToken))
        {
            versions.AddRange(await job);
        }

        return versions;
    }
}
