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
            var packageName = candidate.NugetDependency.NugetPackage.GetPackageName();
            var toVersion = candidate.PossiblePackageVersion.PackageVersion.GetSerializedVersion();
            
            var (title, _) = GetTitleInfo(packageName, toVersion, updateResult);
            
            return title;
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

        var lines = new List<string>();
        foreach (var (framework, fromVersion) in updateResult.FromVersionsPerFramework)
        {
            lines.Add($"Bump {packageName} from {fromVersion.GetSerializedVersion()} to {toVersion} for {framework}");
        }
        
        return (string.Join("\n", lines), true);
    }
}
