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

    /// <summary>
    /// Gets all target frameworks that are compatible with the given target frameworks.
    /// For example, for net10.0, this returns net10.0 and netstandard frameworks.
    /// This is used for version conflict checking across compatible frameworks.
    /// </summary>
    public IEnumerable<string> GetCompatibleTargetFrameworks(
        IReadOnlyCollection<NugetTargetFramework> targetFrameworks,
        IEnumerable<string> availableFrameworks
    )
    {
        var compatibleFrameworks = new HashSet<string>();
        
        foreach (var targetFramework in targetFrameworks)
        {
            var nugetFramework = NuGetFramework.Parse(
                targetFramework.TargetFramework,
                DefaultFrameworkNameProvider.Instance
            );
            
            // Add the target framework itself
            compatibleFrameworks.Add(targetFramework.TargetFramework);
            
            // Find all available frameworks that are compatible with this target framework
            foreach (var availableFramework in availableFrameworks)
            {
                var availableNugetFramework = NuGetFramework.Parse(
                    availableFramework,
                    DefaultFrameworkNameProvider.Instance
                );
                
                // Check if the available framework is compatible with the target framework
                // This allows netstandard packages to be considered for net10.0 targets
                if (DefaultCompatibilityProvider.Instance.IsCompatible(nugetFramework, availableNugetFramework))
                {
                    compatibleFrameworks.Add(availableFramework);
                }
            }
        }
        
        return compatibleFrameworks;
    }
}
