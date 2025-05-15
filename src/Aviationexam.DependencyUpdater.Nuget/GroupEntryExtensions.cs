using Aviationexam.DependencyUpdater.Common;
using System.Collections.Generic;
using System.Linq;

namespace Aviationexam.DependencyUpdater.Nuget;

public static class GroupEntryExtensions
{
    public static string GetTitle<T>(
        this GroupEntry groupEntry,
        IReadOnlyCollection<NugetUpdateCandidate<T>> nugetUpdateCandidates
    )
    {
        if (
            nugetUpdateCandidates.Count == 1
            && nugetUpdateCandidates.Single() is { } candidate
        )
        {
            var name = candidate.NugetDependency.NugetPackage.GetPackageName();
            var from = candidate.NugetDependency.NugetPackage.GetVersion()?.GetSerializedVersion() ?? "unknown";
            var to = candidate.PackageVersion.GetSerializedVersion();
            return $"Bump {name} from {from} to {to}";
        }

        var pluralSuffix = nugetUpdateCandidates.Count == 1 ? "" : "s";
        return $"Bump {groupEntry.GroupName} group – {nugetUpdateCandidates.Count} update{pluralSuffix}";
    }
}
