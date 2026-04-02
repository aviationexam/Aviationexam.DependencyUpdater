using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.DependencyGraph.Models;
using Aviationexam.DependencyUpdater.Nuget.Models;
using System.Collections.Generic;

namespace Aviationexam.DependencyUpdater.Nuget.DependencyGraph.Services;

public sealed class DependencyGraphBuilder
{
    private readonly Dictionary<(string PackageName, PackageVersion Version), DependencyGraphNode> _nodes = new();
    private readonly List<DependencyGraphEdge> _edges = [];
    private readonly List<ProjectDependencyLink> _projectLinks = [];

    public DependencyGraphNode AddOrGetNode(string packageName, PackageVersion version, bool isMetadataAvailable = true)
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
