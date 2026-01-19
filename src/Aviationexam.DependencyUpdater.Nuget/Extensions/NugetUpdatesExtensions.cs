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
        this T updatedPackages
    ) where T : IReadOnlyCollection<NugetUpdateResult>
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine($"Updates {updatedPackages.Count} packages:");
        stringBuilder.AppendLine();

        foreach (var updateResult in updatedPackages)
        {
            var packageName = updateResult.UpdateCandidate.NugetDependency.NugetPackage.GetPackageName();
            var toVersion = updateResult.UpdateCandidate.PossiblePackageVersion.PackageVersion.GetSerializedVersion();
            
            var (fromVersion, frameworkSuffix) = GetFromVersionAndFrameworkSuffix(updateResult);
            
            stringBuilder.AppendLine(
                $"- Update {packageName} from {fromVersion} to {toVersion}{frameworkSuffix}"
            );
        }

        if (stringBuilder.Length == 0)
        {
            return null;
        }

        return stringBuilder.ToString();
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
