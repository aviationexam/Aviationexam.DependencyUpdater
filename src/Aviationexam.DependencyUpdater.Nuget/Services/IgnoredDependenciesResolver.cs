using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.Models;
using System.Collections.Generic;
using ZLinq;
using PackageDependencyInfo = Aviationexam.DependencyUpdater.Common.PackageDependencyInfo;

namespace Aviationexam.DependencyUpdater.Nuget.Services;

public sealed class IgnoredDependenciesResolver
{
    public bool IsDependencyIgnored(
        PackageDependencyInfo packageDependency,
        IgnoreResolver ignoreResolver,
        NugetPackageCondition condition,
        IReadOnlyDictionary<string, IDictionary<NugetPackageCondition, IDictionary<NugetTargetFrameworkGroup, PackageVersion>>> currentPackageVersionsPerTargetFramework,
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
                             && packageVersions.TryGetValue(condition, out var targetFrameworkVersions)
            ? targetFrameworkVersions
                .AsValueEnumerable()
                .Where(v => v.Key.CanBeUsedWith(targetFramework.TargetFramework, out _))
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
