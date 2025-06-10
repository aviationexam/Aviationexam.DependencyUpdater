using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.Services;
using NuGet.Versioning;

namespace Aviationexam.DependencyUpdater.Nuget.Extensions;

public static class NuGetVersionExtensions
{
    public static PackageVersion MapToPackageVersion(
        this NuGetVersion nuGetVersion
    ) => new(
        nuGetVersion.Version,
        nuGetVersion.IsPrerelease,
        [.. nuGetVersion.ReleaseLabels],
        NugetReleaseLabelComparer.Instance
    );
}
