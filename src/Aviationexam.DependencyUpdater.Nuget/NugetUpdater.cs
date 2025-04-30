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
    DirectoryPackagesPropsParser directoryPackagesPropsParser,
    CsprojParser csprojParser,
    NugetVersionFetcherFactory nugetVersionFetcherFactory,
    NugetVersionFetcher nugetVersionFetcher,
    ILogger<NugetUpdater> logger
)
{
    public async Task ProcessUpdatesAsync(
        string directoryPath,
        IReadOnlyCollection<NugetFeedAuthentication> nugetFeedAuthentications,
        CancellationToken cancellationToken = default
    )
    {
        var nugetUpdaterContext = CreateContext(
            directoryPath
        );

        var sourceRepositories = nugetUpdaterContext.NugetConfigurations
            .GroupBy(x => x.Source)
            .Select(x => x.First())
            .ToDictionary(
                x => x,
                x => nugetVersionFetcherFactory.CreateSourceRepository(x, nugetFeedAuthentications)
            );

        var dependencies = nugetUpdaterContext.MapSourceToDependency(logger);
        foreach (var (dependency, sources) in dependencies)
        {
            var versions = await FetchDependencyVersionsAsync(
                dependency,
                sources,
                sourceRepositories,
                cancellationToken
            );
        }
    }

    public NugetUpdaterContext CreateContext(string directoryPath)
    {
        var nugetConfigurations = nugetFinder.GetNugetConfig(directoryPath)
            .SelectMany(nugetConfigParser.Parse)
            .ToList();

        var dependencies = nugetFinder.GetDirectoryPackagesPropsFiles(directoryPath)
            .SelectMany(directoryPackagesPropsParser.Parse)
            .Concat(
                nugetFinder.GetAllCsprojFiles(directoryPath)
                    .SelectMany(csprojParser.Parse)
                    .Where(x => x.NugetPackage is NugetPackageReference { VersionRange: not null })
            )
            .ToList();

        return new NugetUpdaterContext(
            nugetConfigurations,
            dependencies
        );
    }

    private async Task<IReadOnlyCollection<KeyValuePair<NugetSource, IPackageSearchMetadata>>> FetchDependencyVersionsAsync(
        NugetDependency dependency,
        IReadOnlyCollection<NugetSource> sources,
        IReadOnlyDictionary<NugetSource, SourceRepository> sourceRepositories,
        CancellationToken cancellationToken
    )
    {
        var versions = new List<KeyValuePair<NugetSource, IPackageSearchMetadata>>();
        var tasks = sources.Select(async nugetSource =>
        {
            if (sourceRepositories.TryGetValue(nugetSource, out var sourceRepository))
            {
                using var nugetCache = new SourceCacheContext();
                var packageVersions = await nugetVersionFetcher.FetchPackageVersionsAsync(sourceRepository, dependency, nugetCache, cancellationToken);
                return packageVersions.Select(x => KeyValuePair.Create(nugetSource, x)).ToList();
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
