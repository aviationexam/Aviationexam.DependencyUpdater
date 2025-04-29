using System.Linq;

namespace Aviationexam.DependencyUpdater.Nuget;

public sealed class NugetUpdater(
    NugetFinder nugetFinder,
    NugetConfigParser nugetConfigParser,
    DirectoryPackagesPropsParser directoryPackagesPropsParser,
    CsprojParser csprojParser
)
{
    public NugetUpdaterContext CreateContext(string directoryPath)
    {
        var nugetConfigurations = nugetFinder.GetAllNugetFiles(directoryPath)
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
