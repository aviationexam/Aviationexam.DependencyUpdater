using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZLinq;

namespace Aviationexam.DependencyUpdater.Nuget.Extensions;

public static class NugetUpdatesExtensions
{
    public static string? GetCommitMessage<T>(
        this T updatedPackages,
        IReadOnlyDictionary<string, IDictionary<string, PackageVersion>> currentPackageVersions
    ) where T : IReadOnlyCollection<NugetUpdateCandidate>
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine($"Updates {updatedPackages.Count} packages:");
        stringBuilder.AppendLine();

        foreach (var updatedPackage in updatedPackages)
        {
            var packageName = updatedPackage.NugetDependency.NugetPackage.GetPackageName();
            var fromVersion = GetMinimumVersionBeingUpdated(updatedPackage, currentPackageVersions)?.GetSerializedVersion();
            var toVersion = updatedPackage.PossiblePackageVersion.PackageVersion.GetSerializedVersion();
            
            stringBuilder.AppendLine(
                $"- Update {packageName} from {fromVersion} to {toVersion}"
            );
        }

        if (stringBuilder.Length == 0)
        {
            return null;
        }

        return stringBuilder.ToString();
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
