using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.Extensions;
using Aviationexam.DependencyUpdater.Nuget.Models;
using Aviationexam.DependencyUpdater.Nuget.Parsers;
using Aviationexam.DependencyUpdater.Nuget.Services;
using System.Collections.Generic;
using ZLinq;

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
            .AsValueEnumerable()
            .SelectMany(x => nugetConfigParser.Parse(repositoryConfig.RepositoryPath, x))
            .ToList();

        var csprojDependencies = nugetFinder.GetAllCsprojFiles(repositoryConfig)
            .AsValueEnumerable()
            .SelectMany(x => nugetCsprojParser.Parse(repositoryConfig.RepositoryPath, x))
            .ToList();

        var dotnetToolsDependencies = nugetFinder.GetDotnetTools(repositoryConfig)
            .AsValueEnumerable()
            .SelectMany(x => dotnetToolsParser.Parse(repositoryConfig.RepositoryPath, x, defaultTargetFrameworks))
            .ToList();

        var packagesTargetFrameworks = csprojDependencies
            .AsValueEnumerable()
            .GroupBy(x => x.NugetPackage.GetPackageName())
            .ToDictionary(
                x => x.Key,
                IReadOnlyCollection<NugetTargetFramework> (x) => x.AsValueEnumerable().SelectMany(y => y.TargetFrameworks).Distinct().ToList()
            );

        var dependencies = nugetFinder.GetDirectoryPackagesPropsFiles(repositoryConfig)
            .AsValueEnumerable()
            .SelectMany(x => nugetDirectoryPackagesPropsParser.Parse(repositoryConfig.RepositoryPath, x, packagesTargetFrameworks, defaultTargetFrameworks))
            .Concat(csprojDependencies.AsValueEnumerable().Where(x => x.NugetPackage is NugetPackageReference { VersionRange: not null }))
            .Concat(dotnetToolsDependencies)
            .ToList();

        return new NugetUpdaterContext(
            nugetConfigurations,
            dependencies
        );
    }
}
