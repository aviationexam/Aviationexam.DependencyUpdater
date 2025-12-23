using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.Models;
using NuGet.Frameworks;
using System.Collections.Generic;
using System.Linq;
using ZLinq;

namespace Aviationexam.DependencyUpdater.Nuget.Services;

public sealed class TargetFrameworksResolver
{
    public IEnumerable<DependencySet> GetCompatibleDependencySets(
        IReadOnlyCollection<DependencySet> dependencySets,
        IReadOnlyCollection<NugetTargetFramework> dependencyTargetFrameworks
    )
    {
        if (!dependencySets.AsValueEnumerable().Any())
        {
            return dependencyTargetFrameworks.AsValueEnumerable().Select(x => new DependencySet(
                x.TargetFramework,
                []
            )).ToList();
        }

        var compatibleDependencySets = dependencySets.AsEnumerable();

        foreach (var dependencyTargetFramework in dependencyTargetFrameworks)
        {
            var targetFramework = NuGetFramework.Parse(
                dependencyTargetFramework.TargetFramework,
                DefaultFrameworkNameProvider.Instance
            );

            compatibleDependencySets = GetCompatibleDependencySets(
                targetFramework, compatibleDependencySets
            );
        }

        return compatibleDependencySets;
    }

    private IEnumerable<DependencySet> GetCompatibleDependencySets(
        NuGetFramework dependencyTargetFramework,
        IEnumerable<DependencySet> dependencySets
    ) => dependencySets.Where(ds =>
    {
        var dsFramework = NuGetFramework.Parse(ds.TargetFramework, DefaultFrameworkNameProvider.Instance);
        return IsCompatible(dependencyTargetFramework, dsFramework);
    });

    private bool IsCompatible(
        NuGetFramework dependencyTargetFramework,
        NuGetFramework packageVersionNuGetFramework
    ) => DefaultCompatibilityProvider.Instance.IsCompatible(
        dependencyTargetFramework, packageVersionNuGetFramework
    );

    /// <summary>
    /// Gets all target frameworks that should be checked for version conflicts.
    /// For example, for net10.0, this returns net10.0 and netstandard frameworks,
    /// but NOT net9.0 or net11.0 (different .NET versions should have independent versions).
    /// This is used for version conflict checking across compatible frameworks.
    /// </summary>
    public IEnumerable<string> GetCompatibleTargetFrameworks(
        IReadOnlyCollection<NugetTargetFramework> targetFrameworks,
        IReadOnlyCollection<string> availableFrameworks
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

            // Find all available frameworks that should be checked for version conflicts
            foreach (var availableFramework in availableFrameworks)
            {
                var availableNugetFramework = NuGetFramework.Parse(
                    availableFramework,
                    DefaultFrameworkNameProvider.Instance
                );

                // Only include frameworks that:
                // 1. Can be consumed by the target framework (target is compatible with available)
                // 2. Are NOT a different version of the same framework family
                //    (e.g., don't include net9.0 when checking net10.0)

                if (DefaultCompatibilityProvider.Instance.IsCompatible(nugetFramework, availableNugetFramework))
                {
                    compatibleFrameworks.Add(availableFramework);
                }
            }
        }

        return compatibleFrameworks;
    }
}
