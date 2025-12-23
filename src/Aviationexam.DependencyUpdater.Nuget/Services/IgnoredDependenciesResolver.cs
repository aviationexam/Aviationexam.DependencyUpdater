using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.Models;
using NuGet.Packaging;
using System.Collections.Generic;
using ZLinq;
using PackageDependencyInfo = Aviationexam.DependencyUpdater.Common.PackageDependencyInfo;

namespace Aviationexam.DependencyUpdater.Nuget.Services;

public sealed class IgnoredDependenciesResolver
{
    public IEnumerable<PackageDependencyGroup> FilterDependencyGroupsRequiringIgnoredPackages(
        IEnumerable<PackageDependencyGroup> packageDependencyGroups,
        IgnoreResolver ignoreResolver,
        IReadOnlyDictionary<string, IDictionary<string, PackageVersion>> currentPackageVersionsPerTargetFramework,
        NugetTargetFramework targetFramework
    )
    {
        foreach (var packageDependencyGroup in packageDependencyGroups)
        {
            var containsIgnoredDependencies = ContainsIgnoredDependencies(
                packageDependencyGroup,
                ignoreResolver,
                currentPackageVersionsPerTargetFramework,
                targetFramework
            );

            if (containsIgnoredDependencies is false)
            {
                yield return packageDependencyGroup;
            }
        }
    }

    private bool ContainsIgnoredDependencies(
        PackageDependencyGroup packageDependencyGroup,
        IgnoreResolver ignoreResolver,
        IReadOnlyDictionary<string, IDictionary<string, PackageVersion>> currentPackageVersionsPerTargetFramework,
        NugetTargetFramework targetFramework
    )
    {
        foreach (var packageDependency in packageDependencyGroup.Packages)
        {
            var isIgnored = IsDependencyIgnored(
                packageDependency,
                ignoreResolver,
                currentPackageVersionsPerTargetFramework,
                targetFramework
            );

            if (isIgnored)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsDependencyIgnored(
        PackageDependencyInfo packageDependency,
        IgnoreResolver ignoreResolver,
        IReadOnlyDictionary<string, IDictionary<string, PackageVersion>> currentPackageVersionsPerTargetFramework,
        NugetTargetFramework targetFramework
    )
    {
        var proposedVersion = packageDependency.MinVersion;

        if (proposedVersion is null)
        {
            return false;
        }

        // Get the current version for this specific target framework
        var currentVersion = currentPackageVersionsPerTargetFramework.TryGetValue(packageDependency.Id, out var packageVersions)
            ? packageVersions
                .AsValueEnumerable()
                .Where(v => v.Key == targetFramework.TargetFramework)
                .Select(kvp => kvp.Value)
                .FirstOrDefault() ?? proposedVersion
            : proposedVersion;

        if (currentVersion > proposedVersion)
        {
            return false;
        }

        var isIgnored = ignoreResolver.IsIgnored(
            packageDependency.Id,
            currentVersion,
            proposedVersion
        );

        if (isIgnored)
        {
            return true;
        }

        return false;
    }
}
