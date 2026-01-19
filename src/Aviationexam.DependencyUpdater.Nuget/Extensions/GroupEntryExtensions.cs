using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.Models;
using System.Collections.Generic;
using ZLinq;

namespace Aviationexam.DependencyUpdater.Nuget.Extensions;

public static class GroupEntryExtensions
{
    public static string GetTitle(
        this GroupEntry groupEntry,
        IReadOnlyCollection<NugetUpdateCandidate> updateResults
    )
    {
        var distinctPackages = updateResults
            .AsValueEnumerable()
            .Select(x => x.NugetDependency.NugetPackage.GetPackageName())
            .Distinct()
            .ToList();

        var allFromVersions = updateResults
            .AsValueEnumerable()
            .Select(x => x.NugetDependency.NugetPackage.GetVersion())
            .Distinct()
            .ToList();
        var allToVersions = updateResults
            .AsValueEnumerable()
            .Select(x => new PackageVersion(x.PossiblePackageVersion.PackageVersion))
            .Distinct()
            .ToList();

        if (distinctPackages is [var packageName])
        {
            var fromVersionRange = allFromVersions is [var singleFromVersion]
                ? singleFromVersion?.GetSerializedVersion() ?? "unknown"
                : $"{allFromVersions.AsValueEnumerable().Min()!.GetSerializedVersion()}-{allFromVersions.AsValueEnumerable().Max()!.GetSerializedVersion()}";

            var toVersionRange = allToVersions is [var singleToVersion]
                ? singleToVersion.GetSerializedVersion()
                : $"{allToVersions.AsValueEnumerable().Min()!.GetSerializedVersion()}-{allToVersions.AsValueEnumerable().Max()!.GetSerializedVersion()}";

            var conditions = updateResults.AsValueEnumerable()
                .Select(x => x.NugetDependency.NugetPackage.GetCondition())
                .Distinct()
                .ToList();

            if (conditions is [var framework] && !string.IsNullOrEmpty(framework))
            {
                return $"Bump {packageName} from {fromVersionRange} to {toVersionRange} for {framework}";
            }

            return $"Bump {packageName} from {fromVersionRange} to {toVersionRange}";
        }

        var suffix = "";
        if (
            allFromVersions is [var groupFromVersion]
            && allToVersions is [var groupToVersion]
        )
        {
            suffix = $" from {groupFromVersion?.GetSerializedVersion() ?? "unknown"} to {groupToVersion.GetSerializedVersion()}";
        }

        var pluralSuffix = updateResults.Count == 1 ? "" : "s";
        return $"Bump {groupEntry.GroupName} group – {updateResults.Count} update{pluralSuffix}{suffix}";
    }
}
