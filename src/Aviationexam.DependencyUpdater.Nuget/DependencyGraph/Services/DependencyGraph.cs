using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.DependencyGraph.Models;
using System;
using System.Collections.Generic;

namespace Aviationexam.DependencyUpdater.Nuget.DependencyGraph.Services;

public sealed class DependencyGraph
{
    private readonly Dictionary<DependencyGraphNode, List<DependencyGraphEdge>> _outgoingEdges;
    private readonly Dictionary<DependencyGraphNode, List<DependencyGraphEdge>> _incomingEdges;
    private readonly Dictionary<string, List<ProjectDependencyLink>> _projectLinks;
    private readonly Dictionary<DependencyGraphNode, List<ProjectDependencyLink>> _nodeLinks;

    internal DependencyGraph(
        IReadOnlyDictionary<(string PackageName, PackageVersion Version), DependencyGraphNode> nodes,
        IReadOnlyCollection<DependencyGraphEdge> edges,
        IReadOnlyCollection<ProjectDependencyLink> projectLinks
    )
    {
        Nodes = nodes;
        Edges = edges;
        ProjectLinks = projectLinks;

        _outgoingEdges = new Dictionary<DependencyGraphNode, List<DependencyGraphEdge>>();
        _incomingEdges = new Dictionary<DependencyGraphNode, List<DependencyGraphEdge>>();
        _projectLinks = new Dictionary<string, List<ProjectDependencyLink>>();
        _nodeLinks = new Dictionary<DependencyGraphNode, List<ProjectDependencyLink>>();

        foreach (var edge in edges)
        {
            if (!_outgoingEdges.TryGetValue(edge.From, out var outgoing))
            {
                outgoing = [];
                _outgoingEdges[edge.From] = outgoing;
            }

            outgoing.Add(edge);

            if (!_incomingEdges.TryGetValue(edge.To, out var incoming))
            {
                incoming = [];
                _incomingEdges[edge.To] = incoming;
            }

            incoming.Add(edge);
        }

        foreach (var link in projectLinks)
        {
            if (!_projectLinks.TryGetValue(link.ProjectName, out var byProject))
            {
                byProject = [];
                _projectLinks[link.ProjectName] = byProject;
            }

            byProject.Add(link);

            if (!_nodeLinks.TryGetValue(link.Node, out var byNode))
            {
                byNode = [];
                _nodeLinks[link.Node] = byNode;
            }

            byNode.Add(link);
        }
    }

    public IReadOnlyDictionary<(string PackageName, PackageVersion Version), DependencyGraphNode> Nodes { get; }

    public IReadOnlyCollection<DependencyGraphEdge> Edges { get; }

    public IReadOnlyCollection<ProjectDependencyLink> ProjectLinks { get; }

    public IReadOnlyCollection<DependencyGraphEdge> GetOutgoingEdges(DependencyGraphNode node)
        => _outgoingEdges.TryGetValue(node, out var edges) ? edges : Array.Empty<DependencyGraphEdge>();

    public IReadOnlyCollection<DependencyGraphEdge> GetIncomingEdges(DependencyGraphNode node)
        => _incomingEdges.TryGetValue(node, out var edges) ? edges : Array.Empty<DependencyGraphEdge>();

    public IReadOnlyCollection<ProjectDependencyLink> GetLinksForProject(string projectName)
        => _projectLinks.TryGetValue(projectName, out var links) ? links : Array.Empty<ProjectDependencyLink>();

    public IReadOnlyCollection<ProjectDependencyLink> GetLinksForNode(DependencyGraphNode node)
        => _nodeLinks.TryGetValue(node, out var links) ? links : Array.Empty<ProjectDependencyLink>();

    public DependencyGraphNode? FindNode(string packageName, PackageVersion version)
        => Nodes.TryGetValue((packageName, version), out var node) ? node : null;
}
