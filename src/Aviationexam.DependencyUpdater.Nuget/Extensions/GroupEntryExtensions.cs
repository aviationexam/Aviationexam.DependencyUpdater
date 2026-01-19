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
        if (
            updateResults.Count == 1
            && updateResults.AsValueEnumerable().Single() is { } updateResult
        )
        {
            var candidate = updateResult.UpdateCandidate;
            var name = candidate.NugetDependency.NugetPackage.GetPackageName();
            var to = candidate.PossiblePackageVersion.PackageVersion.GetSerializedVersion();
            
            var (from, frameworkSuffix) = GetFromVersionAndFrameworkSuffix(updateResult);
            
            return $"Bump {name} from {from} to {to}{frameworkSuffix}";
        }

        var pluralSuffix = updateResults.Count == 1 ? "" : "s";
        return $"Bump {groupEntry.GroupName} group – {updateResults.Count} update{pluralSuffix}";
    }

    private static (string FromVersion, string FrameworkSuffix) GetFromVersionAndFrameworkSuffix(
        NugetUpdateResult updateResult
    )
    {
        if (updateResult.FromVersionsPerFramework.Count == 0)
        {
            var fallbackVersion = updateResult.UpdateCandidate.NugetDependency.NugetPackage.GetVersion()?.GetSerializedVersion() ?? "unknown";
            return (fallbackVersion, string.Empty);
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
                return (fromVersion, $" for {framework}");
            }
            
            return (fromVersion, string.Empty);
        }

        var minVersion = uniqueVersions.AsValueEnumerable().Min()!.GetSerializedVersion();
        var maxVersion = uniqueVersions.AsValueEnumerable().Max()!.GetSerializedVersion();
        return ($"{minVersion}-{maxVersion}", string.Empty);
    }
}
