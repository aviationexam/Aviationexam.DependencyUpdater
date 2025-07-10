using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.Extensions;
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
    NugetCsprojParser nugetCsprojParser,
    DotnetToolsParser dotnetToolsParser
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

        var csprojDependencies = nugetFinder.GetAllCsprojFiles(repositoryConfig)
            .SelectMany(x => nugetCsprojParser.Parse(repositoryConfig.RepositoryPath, x))
            .ToList();

        var dotnetToolsDependencies = nugetFinder.GetDotnetTools(repositoryConfig)
            .SelectMany(x => dotnetToolsParser.Parse(repositoryConfig.RepositoryPath, x))
            .ToList();

        var packagesTargetFrameworks = csprojDependencies
            .GroupBy(x => x.NugetPackage.GetPackageName())
            .ToDictionary(
                x => x.Key,
                IReadOnlyCollection<NugetTargetFramework> (x) => x.SelectMany(y => y.TargetFrameworks).Distinct().ToList()
            );

        var dependencies = nugetFinder.GetDirectoryPackagesPropsFiles(repositoryConfig)
            .SelectMany(x => nugetDirectoryPackagesPropsParser.Parse(repositoryConfig.RepositoryPath, x, packagesTargetFrameworks, defaultTargetFrameworks))
            .Concat(csprojDependencies.Where(x => x.NugetPackage is NugetPackageReference { VersionRange: not null }))
            .Concat(dotnetToolsDependencies)
            .ToList();

        return new NugetUpdaterContext(
            nugetConfigurations,
            dependencies
        );
    }
}
