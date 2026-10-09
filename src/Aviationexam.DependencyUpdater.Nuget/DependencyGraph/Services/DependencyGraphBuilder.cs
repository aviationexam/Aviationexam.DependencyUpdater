using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.DependencyGraph.Models;
using Aviationexam.DependencyUpdater.Nuget.Extensions;
using Aviationexam.DependencyUpdater.Nuget.Models;
using System.Collections.Generic;

namespace Aviationexam.DependencyUpdater.Nuget.DependencyGraph.Services;

public sealed class DependencyGraphBuilder
{
    private readonly Dictionary<(string PackageName, PackageVersion Version), DependencyGraphNode> _nodes = new();
    private readonly List<DependencyGraphEdge> _edges = [];
    private readonly List<ProjectDependencyLink> _projectLinks = [];

    public DependencyGraphNode AddOrGetNode(INugetPackage nugetPackage, bool isMetadataAvailable = true)
    {
        var packageName = nugetPackage.GetPackageName();
        var version = nugetPackage.GetVersion();

        if (version is null)
        {
            throw new System.ArgumentException($"Package {packageName} has no version", nameof(nugetPackage));
        }

        return AddOrGetNode(packageName, version, isMetadataAvailable);
    }

    public DependencyGraphNode AddOrGetNode(DependencyGraphNode existingNode)
        => AddOrGetNode(existingNode.PackageName, existingNode.Version, existingNode.IsMetadataAvailable);

    private DependencyGraphNode AddOrGetNode(string packageName, PackageVersion version, bool isMetadataAvailable = true)
    {
        var key = (packageName, version);

        if (_nodes.TryGetValue(key, out var existing))
        {
            if (!isMetadataAvailable && existing.IsMetadataAvailable)
            {
                var updated = existing with { IsMetadataAvailable = false };
                _nodes[key] = updated;

                return updated;
            }

            return existing;
        }

        var node = new DependencyGraphNode(packageName, version, isMetadataAvailable);
        _nodes[key] = node;

        return node;
    }

    public DependencyGraphEdge AddEdge(
        DependencyGraphNode from,
        DependencyGraphNode to,
        IReadOnlyCollection<NugetTargetFramework> targetFrameworks
    )
    {
        var edge = new DependencyGraphEdge(from, to, targetFrameworks);
        _edges.Add(edge);

        return edge;
    }

    public void AddProjectLink(ProjectDependencyLink link) => _projectLinks.Add(link);

    public DependencyGraph Build() => new(
        new Dictionary<(string, PackageVersion), DependencyGraphNode>(_nodes),
        _edges.ToArray(),
        _projectLinks.ToArray()
    );
}
