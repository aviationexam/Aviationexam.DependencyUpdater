using System.Collections.Generic;

namespace Aviationexam.DependencyUpdater.Nuget.DependencyGraph.Models;

public sealed record ProjectDependencyLink(
    string ProjectName,
    DependencyGraphNode Node,
    EDependencyLinkNature Nature,
    IReadOnlyList<string> TransitiveChain
);
