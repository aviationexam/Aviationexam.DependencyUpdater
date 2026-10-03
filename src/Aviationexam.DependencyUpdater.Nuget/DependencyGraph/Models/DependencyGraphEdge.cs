using Aviationexam.DependencyUpdater.Nuget.Models;
using System.Collections.Generic;

namespace Aviationexam.DependencyUpdater.Nuget.DependencyGraph.Models;

public sealed record DependencyGraphEdge(
    DependencyGraphNode From,
    DependencyGraphNode To,
    IReadOnlyCollection<NugetTargetFramework> TargetFrameworks
);
