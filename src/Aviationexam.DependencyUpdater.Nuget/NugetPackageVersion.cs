namespace Aviationexam.DependencyUpdater.Nuget;

public sealed record NugetPackageVersion(
    string Name,
    string Version
) : INugetPackage;
