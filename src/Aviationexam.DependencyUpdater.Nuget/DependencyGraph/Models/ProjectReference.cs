using Aviationexam.DependencyUpdater.Nuget.Models;
using System.Collections.Generic;

namespace Aviationexam.DependencyUpdater.Nuget.DependencyGraph.Models;

public sealed record ProjectReference(
    NugetFile NugetFile,
    string ProjectName,
    string RelativePath,
    IReadOnlyCollection<NugetTargetFramework> TargetFrameworks,
    string? Condition = null
);
