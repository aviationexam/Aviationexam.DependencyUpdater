using Aviationexam.DependencyUpdater.Common;
using System;

namespace Aviationexam.DependencyUpdater.Nuget.Extensions;

public static class NugetPackageExtensions
{
    public static string GetPackageName(
        this INugetPackage nugetPackage
    ) => nugetPackage switch
    {
        NugetPackageReference nugetPackageReference => nugetPackageReference.Name,
        NugetPackageVersion nugetPackageVersion => nugetPackageVersion.Name,
        _ => throw new ArgumentOutOfRangeException(nameof(nugetPackage), nugetPackage, null),
    };

    public static PackageVersion? GetVersion(
        this INugetPackage nugetPackage
    ) => nugetPackage switch
    {
        NugetPackageReference nugetPackageReference => nugetPackageReference.VersionRange?.MinVersion?.MapToPackageVersion(),
        NugetPackageVersion nugetPackageVersion => nugetPackageVersion.Version.MapToPackageVersion(),
        _ => throw new ArgumentOutOfRangeException(nameof(nugetPackage), nugetPackage, null),
    };
}
