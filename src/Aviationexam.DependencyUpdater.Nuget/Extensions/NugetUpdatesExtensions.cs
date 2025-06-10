using Aviationexam.DependencyUpdater.Nuget.Models;
using NuGet.Protocol;
using System.Collections.Generic;
using System.Text;

namespace Aviationexam.DependencyUpdater.Nuget.Extensions;

public static class NugetUpdatesExtensions
{
    public static string? GetCommitMessage<T>(
        this T updatedPackages
    ) where T : IReadOnlyCollection<NugetUpdateCandidate<PackageSearchMetadataRegistration>>
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine($"Updates {updatedPackages.Count} packages:");
        stringBuilder.AppendLine();

        foreach (var updatedPackage in updatedPackages)
        {
            stringBuilder.AppendLine(
                $"- Update {updatedPackage.NugetDependency.NugetPackage.GetPackageName()} from {updatedPackage.NugetDependency.NugetPackage.GetVersion()?.GetSerializedVersion()} to {updatedPackage.PackageVersion.GetSerializedVersion()}"
            );
        }

        if (stringBuilder.Length == 0)
        {
            return null;
        }

        return stringBuilder.ToString();
    }
}
