using System.Collections.Generic;

namespace Aviationexam.DependencyUpdater.Nuget;

public sealed record NugetDependency(
    NugetFile NugetFile,
    INugetPackage NugetPackage,
    IReadOnlyCollection<NugetTargetFramework> TargetFrameworks
);
