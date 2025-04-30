using Microsoft.Extensions.Logging;
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

        var sourceRepositories = nugetUpdaterContext.NugetConfigurations.ToDictionary(
            x => x,
            x => nugetVersionFetcherFactory.CreateSourceRepository(x, nugetFeedAuthentications)
        );

        var dependencies = nugetUpdaterContext.MapSourceToDependency(logger);
        foreach (var (dependency, sources) in dependencies)
        {
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
}
