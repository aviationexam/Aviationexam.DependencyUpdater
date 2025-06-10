using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.Models;
using Aviationexam.DependencyUpdater.Nuget.Parsers;
using Aviationexam.DependencyUpdater.Nuget.Services;
using System.Collections.Generic;
using System.Linq;

namespace Aviationexam.DependencyUpdater.Nuget.Factories;

public sealed class NugetContextFactory(
    NugetFinder nugetFinder,
    NugetConfigParser nugetConfigParser,
    NugetDirectoryPackagesPropsParser nugetDirectoryPackagesPropsParser,
    NugetCsprojParser nugetCsprojParser
)
{
    public NugetUpdaterContext CreateContext(
        RepositoryConfig repositoryConfig,
        IReadOnlyCollection<NugetTargetFramework> defaultTargetFrameworks
    )
    {
        var nugetConfigurations = nugetFinder.GetNugetConfig(repositoryConfig)
            .SelectMany(x => nugetConfigParser.Parse(repositoryConfig.RepositoryPath, x))
            .ToList();

        var dependencies = nugetFinder.GetDirectoryPackagesPropsFiles(repositoryConfig)
            .SelectMany(x => nugetDirectoryPackagesPropsParser.Parse(repositoryConfig.RepositoryPath, x, defaultTargetFrameworks))
            .Concat(
                nugetFinder.GetAllCsprojFiles(repositoryConfig)
                    .SelectMany(x => nugetCsprojParser.Parse(repositoryConfig.RepositoryPath, x))
                    .Where(x => x.NugetPackage is NugetPackageReference { VersionRange: not null })
            )
            .ToList();

        return new NugetUpdaterContext(
            nugetConfigurations,
            dependencies
        );
    }
}
