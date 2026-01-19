using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.Models;
using System.Collections.Generic;
using System.Linq;
using ZLinq;

namespace Aviationexam.DependencyUpdater.Nuget.Extensions;

public static class GroupEntryExtensions
{
    public static string GetTitle(
        this GroupEntry groupEntry,
        IReadOnlyCollection<NugetUpdateResult> updateResults
    )
    {
        var distinctPackages = updateResults
            .AsValueEnumerable()
            .Select(x => x.UpdateCandidate.NugetDependency.NugetPackage.GetPackageName())
            .Distinct()
            .ToList();

        if (distinctPackages is [var packageName])
        {
            var allFromVersions = updateResults
                .AsValueEnumerable()
                .SelectMany(x => x.FromVersionsPerFramework.Values)
                .Distinct()
                .ToList();
            var allToVersions = updateResults
                .AsValueEnumerable()
                .Select( (x) => new PackageVersion(x.UpdateCandidate.PossiblePackageVersion.PackageVersion))
                .Distinct()
                .ToList();

            if (allFromVersions.Count == 0)
            {
                var fallbackVersion = updateResults.First().UpdateCandidate.NugetDependency.NugetPackage.GetVersion()?.GetSerializedVersion() ?? "unknown";
                var toVersion = allToVersions is [var version]
                    ? version.GetSerializedVersion()
                    : $"{allToVersions.AsValueEnumerable().Min()!.GetSerializedVersion()}-{allToVersions.AsValueEnumerable().Max()!.GetSerializedVersion()}";
                return $"Bump {packageName} from {fallbackVersion} to {toVersion}";
            }

            var fromVersionRange = allFromVersions is [var singleFromVersion]
                ? singleFromVersion.GetSerializedVersion()
                : $"{allFromVersions.AsValueEnumerable().Min()!.GetSerializedVersion()}-{allFromVersions.AsValueEnumerable().Max()!.GetSerializedVersion()}";

            var toVersionRange = allToVersions is [var singleToVersion]
                ? singleToVersion.GetSerializedVersion()
                : $"{allToVersions.AsValueEnumerable().Min()!.GetSerializedVersion()}-{allToVersions.AsValueEnumerable().Max()!.GetSerializedVersion()}";

            var condition = updateResults.First().UpdateCandidate.NugetDependency.NugetPackage.GetCondition();
            if (!string.IsNullOrWhiteSpace(condition))
            {
                var allFrameworks = updateResults
                    .AsValueEnumerable()
                    .SelectMany(x => x.FromVersionsPerFramework.Keys)
                    .Distinct()
                    .ToList();

                if (allFrameworks is [var framework])
                {
                    return $"Bump {packageName} from {fromVersionRange} to {toVersionRange} for {framework}";
                }
            }

            return $"Bump {packageName} from {fromVersionRange} to {toVersionRange}";
        }

        var pluralSuffix = updateResults.Count == 1 ? "" : "s";
        return $"Bump {groupEntry.GroupName} group – {updateResults.Count} update{pluralSuffix}";
    }
}
