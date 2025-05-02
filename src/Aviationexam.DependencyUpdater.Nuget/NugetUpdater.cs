using Aviationexam.DependencyUpdater.Common;
using Microsoft.Extensions.Logging;
using NuGet.Protocol.Core.Types;
using System.Collections.Generic;
using System.Linq;
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

        var sourceRepositories = nugetUpdaterContext.NugetConfigurations
            .GroupBy(x => x.Source)
            .Select(x => x.First())
            .ToDictionary(
                x => x,
                x => nugetVersionFetcherFactory.CreateSourceRepository(x, nugetFeedAuthentications)
            );

        var ignoreResolver = ignoreResolverFactory.Create(ignoreEntries);

        var dependencies = nugetUpdaterContext.MapSourceToDependency(logger);
        foreach (var (dependency, sources) in dependencies)
        {
            var versions = await FetchDependencyVersionsAsync(
                dependency,
                sources,
                sourceRepositories,
                cancellationToken
            );

            futureVersionResolver.ResolveFutureVersion(
                dependency.NugetPackage.GetPackageName(),
                dependency.NugetPackage.GetVersion(),
                versions.Select(NugetMapper.MapToPackageVersion),
                ignoreResolver
            );
        }
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
