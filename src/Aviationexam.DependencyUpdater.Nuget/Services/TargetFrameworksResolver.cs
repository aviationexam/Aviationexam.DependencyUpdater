using Aviationexam.DependencyUpdater.Nuget.Models;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Protocol;
using System.Collections.Generic;
using System.Linq;
using ZLinq;

namespace Aviationexam.DependencyUpdater.Nuget.Services;

public sealed class TargetFrameworksResolver
{
    public IEnumerable<PackageDependencyGroup> GetCompatiblePackageDependencyGroups(
        PackageSearchMetadataRegistration packageSearchMetadataRegistration,
        IReadOnlyCollection<NugetTargetFramework> dependencyTargetFrameworks
    )
    {
        if (!packageSearchMetadataRegistration.DependencySets.AsValueEnumerable().Any())
        {
            return dependencyTargetFrameworks.AsValueEnumerable().Select(x => new PackageDependencyGroup(NuGetFramework.Parse(
                x.TargetFramework,
                DefaultFrameworkNameProvider.Instance
            ), [])).ToList();
        }

        var compatiblePackageDependencyGroups = packageSearchMetadataRegistration.DependencySets;

        foreach (var dependencyTargetFramework in dependencyTargetFrameworks)
        {
            var tarGetFramework = NuGetFramework.Parse(
                dependencyTargetFramework.TargetFramework,
                DefaultFrameworkNameProvider.Instance
            );

            compatiblePackageDependencyGroups = GetCompatiblePackageDependencyGroups(
                tarGetFramework, compatiblePackageDependencyGroups
            );
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
