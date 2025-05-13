using Aviationexam.DependencyUpdater.Common;

namespace Aviationexam.DependencyUpdater.Nuget;

public record NugetUpdateCandidate<T>(
    NugetDependency NugetDependency,
    PackageVersion<T> PackageVersion
);
