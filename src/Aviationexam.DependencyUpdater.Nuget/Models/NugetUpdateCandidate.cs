using Aviationexam.DependencyUpdater.Common;

namespace Aviationexam.DependencyUpdater.Nuget.Models;

public record NugetUpdateCandidate<TOriginalReference>(
    NugetDependency NugetDependency,
    PackageVersion<TOriginalReference> PackageVersion
);
