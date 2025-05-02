using Aviationexam.DependencyUpdater.Common;
using NuGet.Packaging;
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
            var proposedVersion = packageDependency.VersionRange.MinVersion?.MapToPackageVersion();

            if (proposedVersion is null)
            {
                continue;
            }

            var currentVersion = currentPackageVersions.GetValueOrDefault(packageDependency.Id, proposedVersion);

            var isIgnored = ignoreResolver.IsIgnored(
                packageDependency.Id,
                currentVersion,
                proposedVersion
            );

            if (isIgnored)
            {
                return true;
            }
        }

        return false;
    }
}
