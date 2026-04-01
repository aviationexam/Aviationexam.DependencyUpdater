using Aviationexam.DependencyUpdater.Common;

namespace Aviationexam.DependencyUpdater.Nuget.DependencyGraph.Models;

public sealed record DependencyGraphNode(
    string PackageName,
    PackageVersion Version
)
{
    public bool IsMetadataAvailable { get; init; } = true;

    public DependencyGraphNode(
        string PackageName,
        PackageVersion Version,
        bool IsMetadataAvailable = true
    ) : this(PackageName, Version) => this.IsMetadataAvailable = IsMetadataAvailable;
}
