namespace Aviationexam.DependencyUpdater.Nuget.Models;

public record NugetUpdateCandidate(
    UpdateCandidate NugetDependency,
    PossiblePackageVersion PossiblePackageVersion
);
