using Aviationexam.DependencyUpdater.Nuget.Models;
using System.Collections.Generic;
using System.Text;
using ZLinq;

namespace Aviationexam.DependencyUpdater.Nuget.Extensions;

public static class NugetUpdatesExtensions
{
    public static string? GetCommitMessage<T>(
        this T updatedPackages
    ) where T : IReadOnlyCollection<NugetUpdateCandidate>
    {
        var stringBuilder = new StringBuilder();

        var distinctPackageCount = updatedPackages
            .AsValueEnumerable()
            .Select(x => x.NugetDependency.NugetDependency.NugetPackage.GetPackageName())
            .Distinct()
            .Count();

        stringBuilder.AppendLine($"Updates {distinctPackageCount} packages:");
        stringBuilder.AppendLine();

        foreach (var grouping in updatedPackages.AsValueEnumerable().GroupBy(x => x.NugetDependency.NugetDependency.NugetPackage.GetPackageName()))
        {
            var packageName = grouping.Key;

            foreach (var nugetUpdateResult in grouping)
            {
                var updateLines = GetUpdateLines(packageName, nugetUpdateResult);

                foreach (var line in updateLines)
                {
                    stringBuilder.AppendLine($"- {line}");
                }
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
        NugetUpdateCandidate updateResult
    )
    {
        var fromVersion = updateResult.NugetDependency.NugetDependency.NugetPackage.GetVersion()?.GetSerializedVersion() ?? "unknown";
        var toVersion = updateResult.PossiblePackageVersion.PackageVersion.GetSerializedVersion();

        var condition = updateResult.NugetDependency.NugetDependency.NugetPackage.GetCondition();

        if (condition != NugetPackageCondition.WithoutCondition)
        {
            yield return $"Update {packageName} from {fromVersion} to {toVersion} for {condition.Condition}";
        }
        else
        {
            yield return $"Update {packageName} from {fromVersion} to {toVersion}";
        }
    }
}
