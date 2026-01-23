using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.Extensions;
using Aviationexam.DependencyUpdater.Nuget.Models;
using System.Collections.Generic;

namespace Aviationexam.DependencyUpdater.Nuget.Services;

/// <summary>
/// Processes dependencies to update and determines which dependencies need further checking.
/// Extracted from DependencyAnalyzer for testability.
/// </summary>
public sealed class DependencyUpdateProcessor(
    IgnoredDependenciesResolver ignoredDependenciesResolver
)
{
    /// <summary>
    /// Processes dependencies to update and populates the package flags and dependencies to check.
    /// </summary>
    public DependencyProcessingResult ProcessDependenciesToUpdate(
        IgnoreResolver ignoreResolver,
        CurrentPackageVersions currentPackageVersions,
        IReadOnlyDictionary<UpdateCandidate, IReadOnlyCollection<PossiblePackageVersion>> dependencyToUpdate
    )
    {
        var packageFlags = new Dictionary<Package, IDictionary<NugetTargetFramework, EDependencyFlag>>();
        var dependenciesToCheck = new Queue<(Package Package, NugetPackageCondition Condition,IReadOnlyCollection<NugetTargetFramework> NugetTargetFrameworks)>();

        foreach (var (dependency, possiblePackageVersions) in dependencyToUpdate)
        {
            if (dependency.NugetDependency.NugetPackage.GetVersion() is { } currentVersion)
            {
                var currentPackage = new Package(dependency.NugetDependency.NugetPackage.GetPackageName(), currentVersion);
                var frameworkFlags = GetOrInitializeFrameworkFlags(packageFlags, currentPackage);

                foreach (var targetFramework in dependency.CurrentTargetFrameworkGroup?.TargetFrameworks ?? [])
                {
                    frameworkFlags[targetFramework] = EDependencyFlag.Valid;
                }

                foreach (var targetFramework in dependency.NugetDependency.TargetFrameworks)
                {
                    frameworkFlags[targetFramework] = EDependencyFlag.Valid;
                }
            }

            foreach (var possiblePackageVersion in possiblePackageVersions)
            {
                foreach (var compatibleDependencySet in possiblePackageVersion.CompatibleDependencySets)
                {
                    ProcessDependencySet(
                        ignoreResolver,
                        dependency.NugetDependency.NugetPackage.GetCondition(),
                        currentPackageVersions,
                        packageFlags,
                        dependenciesToCheck,
                        compatibleDependencySet
                    );
                }
            }
        }

        return new DependencyProcessingResult(
            packageFlags,
            dependenciesToCheck
        );
    }

    /// <summary>
    /// Processes a single dependency set and updates package flags.
    /// Returns the list of dependent packages found.
    /// </summary>
    public IReadOnlyCollection<Package> ProcessDependencySet(
        IgnoreResolver ignoreResolver,
        NugetPackageCondition condition,
        CurrentPackageVersions currentPackageVersions,
        IDictionary<Package, IDictionary<NugetTargetFramework, EDependencyFlag>> packageFlags,
        Queue<(Package Package, NugetPackageCondition Condition, IReadOnlyCollection<NugetTargetFramework> NugetTargetFrameworks)> dependenciesToCheck,
        DependencySet compatibleDependencySet
    )
    {
        var dependentPackages = new List<Package>();

        foreach (var packageDependency in compatibleDependencySet.Packages)
        {
            if (packageDependency.MinVersion is not { } minVersion)
            {
                continue;
            }

            var dependentPackage = new Package(packageDependency.Id, minVersion);
            dependentPackages.Add(dependentPackage);

            var frameworkFlags = GetOrInitializeFrameworkFlags(packageFlags, dependentPackage);

            var shouldCheckDependency = ProcessTargetFrameworks(
                ignoreResolver,
                condition,
                currentPackageVersions,
                packageDependency,
                dependentPackage,
                frameworkFlags,
                new NugetTargetFramework(compatibleDependencySet.TargetFramework)
            );

            if (shouldCheckDependency)
            {
                dependenciesToCheck.Enqueue((dependentPackage, condition, [new NugetTargetFramework(compatibleDependencySet.TargetFramework)]));
            }
        }

        return dependentPackages;
    }

    private static IDictionary<NugetTargetFramework, EDependencyFlag> GetOrInitializeFrameworkFlags(
        IDictionary<Package, IDictionary<NugetTargetFramework, EDependencyFlag>> packageFlags,
        Package dependentPackage
    )
    {
        if (!packageFlags.TryGetValue(dependentPackage, out var frameworkFlags))
        {
            frameworkFlags = new Dictionary<NugetTargetFramework, EDependencyFlag>();
            packageFlags[dependentPackage] = frameworkFlags;
        }

        return frameworkFlags;
    }

    /// <summary>
    /// Processes target frameworks for a given package dependency.
    /// Returns true if the dependency should be checked further.
    /// </summary>
    private bool ProcessTargetFrameworks(
        IgnoreResolver ignoreResolver,
        NugetPackageCondition condition,
        CurrentPackageVersions currentPackageVersions,
        PackageDependencyInfo packageDependency,
        Package dependentPackage,
        IDictionary<NugetTargetFramework, EDependencyFlag> frameworkFlags,
        NugetTargetFramework targetFramework
    )
    {
        if (frameworkFlags.ContainsKey(targetFramework))
        {
            return false;
        }

        var shouldCheckDependency = false;

        var flag = DetermineFrameworkFlag(
            ignoreResolver,
            condition,
            currentPackageVersions,
            packageDependency,
            dependentPackage,
            targetFramework
        );

        frameworkFlags[targetFramework] = flag;

        if (flag == EDependencyFlag.Unknown)
        {
            shouldCheckDependency = true;
        }

        return shouldCheckDependency;
    }

    /// <summary>
    /// Determines the dependency flag for a specific target framework.
    /// </summary>
    private EDependencyFlag DetermineFrameworkFlag(
        IgnoreResolver ignoreResolver,
        NugetPackageCondition condition,
        CurrentPackageVersions currentPackageVersions,
        PackageDependencyInfo packageDependency,
        Package dependentPackage,
        NugetTargetFramework targetFramework
    )
    {
        if (currentPackageVersions.IsAtVersion(packageDependency.Id, condition, targetFramework, dependentPackage.Version))
        {
            return EDependencyFlag.Valid;
        }

        var isDependencyIgnored = ignoredDependenciesResolver.IsDependencyIgnored(
            packageDependency,
            ignoreResolver,
            condition,
            currentPackageVersions,
            targetFramework
        );

        return isDependencyIgnored
            ? EDependencyFlag.ContainsIgnoredDependency
            : EDependencyFlag.Unknown;
    }
}
