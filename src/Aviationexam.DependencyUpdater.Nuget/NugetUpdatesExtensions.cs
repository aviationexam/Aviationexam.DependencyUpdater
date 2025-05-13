using Aviationexam.DependencyUpdater.Common;
using System.Collections.Generic;
using System.Text;

namespace Aviationexam.DependencyUpdater.Nuget;

public static class NugetUpdatesExtensions
{
    public static string? GetCommitMessage<T>(
        this T updatedPackages
    ) where T : IReadOnlyCollection<(NugetDependency NugetDependency, PackageVersion PackageVersion)>
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine($"Updates {updatedPackages.Count} packages:");
        stringBuilder.AppendLine();

        foreach (var updatedPackage in updatedPackages)
        {
            stringBuilder.AppendLine(
                $"- Update {updatedPackage.NugetDependency.NugetPackage.GetPackageName()} from {updatedPackage.NugetDependency.NugetPackage.GetVersion()} to {updatedPackage.PackageVersion}"
            );
        }

        if (stringBuilder.Length == 0)
        {
            return null;
        }

        return stringBuilder.ToString();
    }
}
