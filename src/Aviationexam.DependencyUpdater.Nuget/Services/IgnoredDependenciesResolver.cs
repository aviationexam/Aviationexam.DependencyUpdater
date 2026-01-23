using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.Models;
using PackageDependencyInfo = Aviationexam.DependencyUpdater.Common.PackageDependencyInfo;

namespace Aviationexam.DependencyUpdater.Nuget.Services;

public sealed class IgnoredDependenciesResolver
{
    public bool IsDependencyIgnored(
        PackageDependencyInfo packageDependency,
        IgnoreResolver ignoreResolver,
        NugetPackageCondition condition,
        CurrentPackageVersions currentPackageVersions,
        NugetTargetFramework targetFramework
    )
    {
        var proposedVersion = packageDependency.MinVersion;

        if (proposedVersion is null)
        {
            return false;
        }

        var currentVersion = currentPackageVersions.TryGetVersion(packageDependency.Id, condition, targetFramework, out var version)
            ? version
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
