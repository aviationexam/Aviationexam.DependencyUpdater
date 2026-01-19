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

        if (distinctPackages.Count == 1)
        {
            var packageName = distinctPackages[0];
            var allFromVersions = updateResults
                .AsValueEnumerable()
                .SelectMany(x => x.FromVersionsPerFramework.Values)
                .Distinct()
                .ToList();
            var allToVersions = updateResults
                .AsValueEnumerable()
                .Select(x => x.UpdateCandidate.PossiblePackageVersion.PackageVersion)
                .Distinct()
                .ToList();

            if (allFromVersions.Count == 0)
            {
                var fallbackVersion = updateResults.First().UpdateCandidate.NugetDependency.NugetPackage.GetVersion()?.GetSerializedVersion() ?? "unknown";
                var toVersion = allToVersions.Count == 1 
                    ? allToVersions[0].GetSerializedVersion() 
                    : $"{allToVersions.AsValueEnumerable().Min()!.GetSerializedVersion()}-{allToVersions.AsValueEnumerable().Max()!.GetSerializedVersion()}";
                return $"Bump {packageName} from {fallbackVersion} to {toVersion}";
            }

            var fromVersionRange = allFromVersions.Count == 1
                ? allFromVersions[0].GetSerializedVersion()
                : $"{allFromVersions.AsValueEnumerable().Min()!.GetSerializedVersion()}-{allFromVersions.AsValueEnumerable().Max()!.GetSerializedVersion()}";

            var toVersionRange = allToVersions.Count == 1
                ? allToVersions[0].GetSerializedVersion()
                : $"{allToVersions.AsValueEnumerable().Min()!.GetSerializedVersion()}-{allToVersions.AsValueEnumerable().Max()!.GetSerializedVersion()}";

            var allFrameworks = updateResults
                .AsValueEnumerable()
                .SelectMany(x => x.FromVersionsPerFramework.Keys)
                .Distinct()
                .ToList();

            if (allFrameworks.Count == 1)
            {
                return $"Bump {packageName} from {fromVersionRange} to {toVersionRange} for {allFrameworks[0]}";
            }

            return $"Bump {packageName} from {fromVersionRange} to {toVersionRange}";
        }

        var pluralSuffix = updateResults.Count == 1 ? "" : "s";
        return $"Bump {groupEntry.GroupName} group – {updateResults.Count} update{pluralSuffix}";
    }

    private static (string Title, bool IsMultiLine) GetTitleInfo(
        string packageName,
        string toVersion,
        NugetUpdateResult updateResult
    )
    {
        if (updateResult.FromVersionsPerFramework.Count == 0)
        {
            var fallbackVersion = updateResult.UpdateCandidate.NugetDependency.NugetPackage.GetVersion()?.GetSerializedVersion() ?? "unknown";
            return ($"Bump {packageName} from {fallbackVersion} to {toVersion}", false);
        }

        var uniqueVersions = updateResult.FromVersionsPerFramework.Values
            .AsValueEnumerable()
            .Distinct()
            .ToList();

        if (uniqueVersions.Count == 1)
        {
            var fromVersion = uniqueVersions[0].GetSerializedVersion();
            
            if (updateResult.FromVersionsPerFramework.Count == 1)
            {
                var framework = updateResult.FromVersionsPerFramework.Keys.Single();
                return ($"Bump {packageName} from {fromVersion} to {toVersion} for {framework}", false);
            }
            
            return ($"Bump {packageName} from {fromVersion} to {toVersion}", false);
        }

        var minVersion = uniqueVersions.AsValueEnumerable().Min()!.GetSerializedVersion();
        var maxVersion = uniqueVersions.AsValueEnumerable().Max()!.GetSerializedVersion();
        return ($"Bump {packageName} from {minVersion}-{maxVersion} to {toVersion}", false);
    }
}
