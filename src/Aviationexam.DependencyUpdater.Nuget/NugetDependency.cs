namespace Aviationexam.DependencyUpdater.Nuget;

public sealed record NugetDependency(
    NugetFile NugetFile,
    INugetPackage NugetPackage
);
