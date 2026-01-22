using Aviationexam.DependencyUpdater.Common;

namespace Aviationexam.DependencyUpdater.Nuget.Models;

public sealed record UpdateCandidate(
    NugetDependency NugetDependency,
    PackageVersionWithDependencySets? CurrentVersion
);
