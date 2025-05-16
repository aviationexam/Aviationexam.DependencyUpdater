using Aviationexam.DependencyUpdater.Common;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using System.Collections.Generic;

namespace Aviationexam.DependencyUpdater.Nuget;

public sealed class IgnoredDependenciesResolver
{
    public IEnumerable<PackageDependencyGroup> FilterDependencyGroupsRequiringIgnoredPackages(
        IEnumerable<PackageDependencyGroup> packageDependencyGroups,
        IgnoreResolver ignoreResolver,
        IReadOnlyDictionary<string, PackageVersion> currentPackageVersions
    )
    {
        foreach (var packageDependencyGroup in packageDependencyGroups)
        {
            var containsIgnoredDependencies = ContainsIgnoredDependencies(
                packageDependencyGroup,
                ignoreResolver,
                currentPackageVersions
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
        IReadOnlyDictionary<string, PackageVersion> currentPackageVersions
    )
    {
        foreach (var packageDependency in packageDependencyGroup.Packages)
        {
            var isIgnored = IsDependencyIgnored(packageDependency, ignoreResolver, currentPackageVersions);

            if (isIgnored)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsDependencyIgnored(
        PackageDependency packageDependency,
        IgnoreResolver ignoreResolver,
        IReadOnlyDictionary<string, PackageVersion> currentPackageVersions
    )
    {
        var proposedVersion = packageDependency.VersionRange.MinVersion?.MapToPackageVersion();

        if (proposedVersion is null)
        {
            return false;
        }

        var currentVersion = currentPackageVersions.GetValueOrDefault(packageDependency.Id, proposedVersion);

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
