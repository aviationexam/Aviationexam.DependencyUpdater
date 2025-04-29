using System;

namespace Aviationexam.DependencyUpdater.Nuget;

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
}
