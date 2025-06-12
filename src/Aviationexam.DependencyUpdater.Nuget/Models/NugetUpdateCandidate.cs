namespace Aviationexam.DependencyUpdater.Nuget.Models;

public record NugetUpdateCandidate(
    NugetDependency NugetDependency,
    PossiblePackageVersion PossiblePackageVersion
);
