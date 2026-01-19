using Aviationexam.DependencyUpdater.Nuget.Models;
using System.Collections.Generic;
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

        var distinctPackageCount = updatedPackages
            .AsValueEnumerable()
            .Select(x => x.UpdateCandidate.NugetDependency.NugetPackage.GetPackageName())
            .Distinct()
            .Count();

        stringBuilder.AppendLine($"Updates {distinctPackageCount} packages:");
        stringBuilder.AppendLine();

        foreach (var updateResult in updatedPackages)
        {
            var packageName = updateResult.UpdateCandidate.NugetDependency.NugetPackage.GetPackageName();
            var toVersion = updateResult.UpdateCandidate.PossiblePackageVersion.PackageVersion.GetSerializedVersion();

            var updateLines = GetUpdateLines(packageName, toVersion, updateResult);

            foreach (var line in updateLines)
            {
                stringBuilder.AppendLine($"- {line}");
            }
        }

        if (stringBuilder.Length == 0)
        {
            return null;
        }

        return stringBuilder.ToString();
    }

    private static IEnumerable<string> GetUpdateLines(
        string packageName,
        string toVersion,
        NugetUpdateResult updateResult
    )
    {
        if (updateResult.FromVersionsPerFramework.Count == 0)
        {
            var fallbackVersion = updateResult.UpdateCandidate.NugetDependency.NugetPackage.GetVersion()?.GetSerializedVersion() ?? "unknown";
            yield return $"Update {packageName} from {fallbackVersion} to {toVersion}";
            yield break;
        }

        var uniqueVersions = updateResult.FromVersionsPerFramework.Values
            .AsValueEnumerable()
            .Distinct()
            .ToList();

        var condition = updateResult.UpdateCandidate.NugetDependency.NugetPackage.GetCondition();

        if (uniqueVersions is [var singleVersion])
        {
            var fromVersion = singleVersion.GetSerializedVersion();

            if (!string.IsNullOrWhiteSpace(condition) && updateResult.FromVersionsPerFramework.Count == 1)
            {
                var framework = updateResult.FromVersionsPerFramework.AsValueEnumerable().Single().Key;
                yield return $"Update {packageName} from {fromVersion} to {toVersion} for {framework}";
            }
            else
            {
                yield return $"Update {packageName} from {fromVersion} to {toVersion}";
            }

            yield break;
        }

        foreach (var (framework, fromVersion) in updateResult.FromVersionsPerFramework)
        {
            yield return $"Update {packageName} from {fromVersion.GetSerializedVersion()} to {toVersion} for {framework}";
        }
    }
}
