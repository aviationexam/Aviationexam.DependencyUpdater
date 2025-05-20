using Aviationexam.DependencyUpdater.Common;
using NuGet.Versioning;

namespace Aviationexam.DependencyUpdater.Nuget.Extensions;

public static class NuGetVersionExtensions
{
    public static PackageVersion MapToPackageVersion(
        this NuGetVersion nuGetVersion,
        EPackageSource packageSource
    ) => new(
        nuGetVersion.Version,
        nuGetVersion.IsPrerelease,
        [.. nuGetVersion.ReleaseLabels],
        packageSource,
        NugetReleaseLabelComparer.Instance
    );
}
