using Aviationexam.DependencyUpdater.Common;
using NuGet.Versioning;

namespace Aviationexam.DependencyUpdater.Nuget;

public static class PackageVersionExtensions
{
    public static NuGetVersion MapToNuGetVersion(
        this PackageVersion packageVersion
    ) => new(packageVersion.Version, string.Join('.', packageVersion.ReleaseLabels));
}
