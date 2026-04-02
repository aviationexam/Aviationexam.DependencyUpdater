using Aviationexam.DependencyUpdater.Common;
using System;

namespace Aviationexam.DependencyUpdater.Nuget.DependencyGraph.Models;

public sealed record DependencyGraphNode(
    string PackageName,
    PackageVersion Version
)
{
    public bool IsMetadataAvailable { get; init; } = true;

    public bool Equals(DependencyGraphNode? other)
        => other is not null
           && PackageName == other.PackageName
           && Version == other.Version;

    public override int GetHashCode()
        => HashCode.Combine(PackageName, Version);

    public DependencyGraphNode(
        string PackageName,
        PackageVersion Version,
        bool IsMetadataAvailable = true
    ) : this(PackageName, Version) => this.IsMetadataAvailable = IsMetadataAvailable;
}
