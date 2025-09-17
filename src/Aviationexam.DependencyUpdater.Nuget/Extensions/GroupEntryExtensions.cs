using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.Models;
using System.Collections.Generic;
using ZLinq;

namespace Aviationexam.DependencyUpdater.Nuget.Extensions;

public static class GroupEntryExtensions
{
    public static string GetTitle(
        this GroupEntry groupEntry,
        IReadOnlyCollection<NugetUpdateCandidate> nugetUpdateCandidates
    )
    {
        if (
            nugetUpdateCandidates.Count == 1
            && nugetUpdateCandidates.AsValueEnumerable().Single() is { } candidate
        )
        {
            var name = candidate.NugetDependency.NugetPackage.GetPackageName();
            var from = candidate.NugetDependency.NugetPackage.GetVersion()?.GetSerializedVersion() ?? "unknown";
            var to = candidate.PossiblePackageVersion.PackageVersion.GetSerializedVersion();
            return $"Bump {name} from {from} to {to}";
        }

        var pluralSuffix = nugetUpdateCandidates.Count == 1 ? "" : "s";
        return $"Bump {groupEntry.GroupName} group – {nugetUpdateCandidates.Count} update{pluralSuffix}";
    }
}
