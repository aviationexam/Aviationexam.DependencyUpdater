using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.Models;
using System.Collections.Generic;
using ZLinq;

namespace Aviationexam.DependencyUpdater.Nuget.Extensions;

public static class GroupEntryExtensions
{
    public static string GetTitle(
        this GroupEntry groupEntry,
        IReadOnlyCollection<NugetUpdateCandidate> nugetUpdateCandidates,
        IReadOnlyDictionary<string, IDictionary<string, PackageVersion>> currentPackageVersions
    )
    {
        if (
            nugetUpdateCandidates.Count == 1
            && nugetUpdateCandidates.AsValueEnumerable().Single() is { } candidate
        )
        {
            var name = candidate.NugetDependency.NugetPackage.GetPackageName();
            var from = GetMinimumVersionBeingUpdated(candidate, currentPackageVersions)?.GetSerializedVersion() ?? "unknown";
            var to = candidate.PossiblePackageVersion.PackageVersion.GetSerializedVersion();
            return $"Bump {name} from {from} to {to}";
        }

        var pluralSuffix = nugetUpdateCandidates.Count == 1 ? "" : "s";
        return $"Bump {groupEntry.GroupName} group – {nugetUpdateCandidates.Count} update{pluralSuffix}";
    }

    private static PackageVersion? GetMinimumVersionBeingUpdated(
        NugetUpdateCandidate candidate,
        IReadOnlyDictionary<string, IDictionary<string, PackageVersion>> currentPackageVersions
    )
    {
        var packageName = candidate.NugetDependency.NugetPackage.GetPackageName();
        var fallbackVersion = candidate.NugetDependency.NugetPackage.GetVersion();
        
        if (!currentPackageVersions.TryGetValue(packageName, out var frameworkVersions))
        {
            return fallbackVersion;
        }

        var versionsAcrossTargetFrameworks = candidate.NugetDependency.TargetFrameworks
            .AsValueEnumerable()
            .Where(tf => frameworkVersions.ContainsKey(tf.TargetFramework))
            .Select(tf => frameworkVersions[tf.TargetFramework])
            .ToList();

        if (versionsAcrossTargetFrameworks.Count == 0)
        {
            return fallbackVersion;
        }

        return versionsAcrossTargetFrameworks.AsValueEnumerable().Min();
    }
}
