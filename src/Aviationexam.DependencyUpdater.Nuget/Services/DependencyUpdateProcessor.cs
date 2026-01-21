using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.Extensions;
using Aviationexam.DependencyUpdater.Nuget.Models;
using NuGet.Frameworks;
using System.Collections.Generic;
using ZLinq;

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
        IReadOnlyDictionary<string, IDictionary<string, PackageVersion>> currentPackageVersionsPerTargetFramework,
        IReadOnlyDictionary<NugetDependency, IReadOnlyCollection<PossiblePackageVersion>> dependencyToUpdate
    )
    {
        var packageFlags = new Dictionary<Package, IDictionary<NugetTargetFramework, EDependencyFlag>>();
        var dependenciesToCheck = new Queue<(Package Package, IReadOnlyCollection<NugetTargetFramework> NugetTargetFrameworks)>();

        foreach (var (dependency, possiblePackageVersions) in dependencyToUpdate)
        {
            if (dependency.NugetPackage.GetVersion() is { } currentVersion)
            {
                var currentPackage = new Package(dependency.NugetPackage.GetPackageName(), currentVersion);
                var frameworkFlags = GetOrInitializeFrameworkFlags(packageFlags, currentPackage);

                foreach (var targetFrameworks in dependency.TargetFrameworks)
                {
                    frameworkFlags[targetFrameworks] = EDependencyFlag.Valid;
                }

                if (possiblePackageVersions.AsValueEnumerable().SingleOrDefault(x => x.IsCurrentVersion) is { } currentVersionPossiblePackageVersion)
                {
                    foreach (var compatibleDependencySet in currentVersionPossiblePackageVersion.CompatibleDependencySets)
                    {
                        frameworkFlags[new NugetTargetFramework(compatibleDependencySet.TargetFramework)] = EDependencyFlag.Valid;
                    }
                }
            }

            foreach (var possiblePackageVersion in possiblePackageVersions)
            {
                if (possiblePackageVersion.IsCurrentVersion)
                {
                    continue;
                }

                foreach (var compatibleDependencySet in possiblePackageVersion.CompatibleDependencySets)
                {
                    ProcessDependencySet(
                        ignoreResolver,
                        currentPackageVersionsPerTargetFramework,
                        packageFlags,
                        dependenciesToCheck,
                        compatibleDependencySet,
                        dependency.TargetFrameworks
                    );
                }
            }
        }

        return new DependencyProcessingResult(packageFlags, dependenciesToCheck);
    }

    /// <summary>
    /// Processes a single dependency set and updates package flags.
    /// Returns the list of dependent packages found.
    /// </summary>
    public IReadOnlyCollection<Package> ProcessDependencySet(
        IgnoreResolver ignoreResolver,
        IReadOnlyDictionary<string, IDictionary<string, PackageVersion>> currentPackageVersionsPerTargetFramework,
        IDictionary<Package, IDictionary<NugetTargetFramework, EDependencyFlag>> packageFlags,
        Queue<(Package Package, IReadOnlyCollection<NugetTargetFramework> NugetTargetFrameworks)> dependenciesToCheck,
        DependencySet compatibleDependencySet,
        IReadOnlyCollection<NugetTargetFramework> targetFrameworks
    )
    {
        var nuGetFramework = NuGetFramework.Parse(compatibleDependencySet.TargetFramework, DefaultFrameworkNameProvider.Instance);

        var isGroupCompatible = targetFrameworks.AsValueEnumerable().Any(x => DefaultCompatibilityProvider.Instance.IsCompatible(
            NuGetFramework.Parse(x.TargetFramework, DefaultFrameworkNameProvider.Instance), nuGetFramework
        ));

        if (!isGroupCompatible)
        {
            return [];
        }

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
                currentPackageVersionsPerTargetFramework,
                packageDependency,
                dependentPackage,
                frameworkFlags,
                targetFrameworks
            );

            if (shouldCheckDependency)
            {
                dependenciesToCheck.Enqueue((dependentPackage, targetFrameworks));
            }
        }

        return dependentPackages;
    }

    private static IDictionary<NugetTargetFramework, EDependencyFlag> GetOrInitializeFrameworkFlags(
        IDictionary<Package, IDictionary<NugetTargetFramework, EDependencyFlag>> packageFlags,
        Package dependentPackage
    )
    {
        // Initialize per-framework flags for this package if needed
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
        IReadOnlyDictionary<string, IDictionary<string, PackageVersion>> currentPackageVersionsPerTargetFramework,
        PackageDependencyInfo packageDependency,
        Package dependentPackage,
        IDictionary<NugetTargetFramework, EDependencyFlag> frameworkFlags,
        IReadOnlyCollection<NugetTargetFramework> targetFrameworks
    )
    {
        var shouldCheckDependency = false;

        foreach (var targetFramework in targetFrameworks)
        {
            if (frameworkFlags.ContainsKey(targetFramework))
            {
                // Already processed for this target framework
                continue;
            }

            var flag = DetermineFrameworkFlag(
                ignoreResolver,
                currentPackageVersionsPerTargetFramework,
                packageDependency,
                dependentPackage,
                targetFramework
            );

            frameworkFlags[targetFramework] = flag;

            if (flag == EDependencyFlag.Unknown)
            {
                shouldCheckDependency = true;
            }
        }

        return shouldCheckDependency;
    }

    /// <summary>
    /// Determines the dependency flag for a specific target framework.
    /// </summary>
    private EDependencyFlag DetermineFrameworkFlag(
        IgnoreResolver ignoreResolver,
        IReadOnlyDictionary<string, IDictionary<string, PackageVersion>> currentPackageVersionsPerTargetFramework,
        PackageDependencyInfo packageDependency,
        Package dependentPackage,
        NugetTargetFramework targetFramework
    )
    {
        // Check if this package is already at the correct version for this target framework
        if (
            IsAtCorrectVersion(
                currentPackageVersionsPerTargetFramework,
                packageDependency.Id,
                dependentPackage.Version,
                targetFramework
            )
        )
        {
            return EDependencyFlag.Valid;
        }

        var isDependencyIgnored = ignoredDependenciesResolver.IsDependencyIgnored(
            packageDependency,
            ignoreResolver,
            currentPackageVersionsPerTargetFramework,
            targetFramework
        );

        return isDependencyIgnored
            ? EDependencyFlag.ContainsIgnoredDependency
            : EDependencyFlag.Unknown;
    }

    /// <summary>
    /// Checks if a package is already at the correct version for a target framework.
    /// </summary>
    private static bool IsAtCorrectVersion(
        IReadOnlyDictionary<string, IDictionary<string, PackageVersion>> currentPackageVersionsPerTargetFramework,
        string packageId,
        PackageVersion expectedVersion,
        NugetTargetFramework targetFramework
    )
    {
        return currentPackageVersionsPerTargetFramework.TryGetValue(packageId, out var frameworkVersions)
               && frameworkVersions.TryGetValue(targetFramework.TargetFramework, out var currentVersion)
               && currentVersion == expectedVersion;
    }
}
