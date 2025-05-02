using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Protocol;
using System.Collections.Generic;
using System.Linq;

namespace Aviationexam.DependencyUpdater.Nuget;

public sealed class TargetFrameworksResolver
{
    public IReadOnlyCollection<PackageDependencyGroup> GetCompatiblePackageDependencyGroups(
        PackageSearchMetadataRegistration packageSearchMetadataRegistration,
        IReadOnlyCollection<NugetTargetFramework> dependencyTargetFrameworks
    )
    {
        var compatiblePackageDependencyGroups = packageSearchMetadataRegistration.DependencySets.ToList();

        foreach (var dependencyTargetFramework in dependencyTargetFrameworks)
        {
            var tarGetFramework = NuGetFramework.Parse(
                dependencyTargetFramework.TargetFramework,
                DefaultFrameworkNameProvider.Instance
            );

            compatiblePackageDependencyGroups = GetCompatiblePackageDependencyGroups(
                tarGetFramework, compatiblePackageDependencyGroups
            ).ToList();

            if (compatiblePackageDependencyGroups.Count == 0)
            {
                return [];
            }
        }

        return compatiblePackageDependencyGroups;
    }

    private IEnumerable<PackageDependencyGroup> GetCompatiblePackageDependencyGroups(
        NuGetFramework dependencyTargetFramework,
        IEnumerable<PackageDependencyGroup> packageVersionDependencyGroups
    ) => packageVersionDependencyGroups.Where(x =>
        IsCompatible(dependencyTargetFramework, x.TargetFramework)
    );

    private bool IsCompatible(
        NuGetFramework dependencyTargetFramework,
        NuGetFramework packageVersionNuGetFramework
    ) => DefaultCompatibilityProvider.Instance.IsCompatible(
        dependencyTargetFramework, packageVersionNuGetFramework
    );
}
