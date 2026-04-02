using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.DependencyGraph.Models;
using Aviationexam.DependencyUpdater.Nuget.DependencyGraph.Services;
using Aviationexam.DependencyUpdater.Nuget.Models;
using Aviationexam.DependencyUpdater.Nuget.Services;
using System;
using Xunit;

namespace Aviationexam.DependencyUpdater.Nuget.Tests.DependencyGraph;

public sealed class DependencyGraphTests
{
    private static PackageVersion CreateVersion(int major, int minor, int patch = 0)
        => new(new Version(major, minor, patch, 0), false, [], NugetReleaseLabelComparer.Instance);

    [Fact]
    public void AddOrGetNode_SameKey_ReturnsSameInstance()
    {
        // Arrange
        var builder = new DependencyGraphBuilder();
        var version = CreateVersion(1, 0);

        // Act
        var first = builder.AddOrGetNode("A", version);
        var second = builder.AddOrGetNode("A", version);

        // Assert
        Assert.Same(first, second);
    }

    [Fact]
    public void EdgeAdditionAndQuery_ReturnsCorrectEdges()
    {
        // Arrange
        var builder = new DependencyGraphBuilder();
        var nodeA = builder.AddOrGetNode("A", CreateVersion(1, 0));
        var nodeB = builder.AddOrGetNode("B", CreateVersion(2, 0));
        var tfms = new[] { new NugetTargetFramework("net9.0") };

        builder.AddEdge(nodeA, nodeB, tfms);

        // Act
        var graph = builder.Build();
        var outgoing = graph.GetOutgoingEdges(nodeA);
        var incoming = graph.GetIncomingEdges(nodeB);

        // Assert
        var outEdge = Assert.Single(outgoing);
        Assert.Equal(nodeA, outEdge.From);
        Assert.Equal(nodeB, outEdge.To);

        var inEdge = Assert.Single(incoming);
        Assert.Equal(nodeA, inEdge.From);
        Assert.Equal(nodeB, inEdge.To);
    }

    [Fact]
    public void Build_SubsequentBuilderChanges_DoNotAffectBuiltGraph()
    {
        // Arrange
        var builder = new DependencyGraphBuilder();
        builder.AddOrGetNode("A", CreateVersion(1, 0));

        var graph = builder.Build();

        // Act - add more to builder after Build()
        var newVersion = CreateVersion(2, 0);
        builder.AddOrGetNode("NewNode", newVersion);

        // Assert - original graph is unaffected
        Assert.Single(graph.Nodes);
        Assert.Null(graph.FindNode("NewNode", newVersion));
    }

    [Fact]
    public void EmptyGraph_AllQueryMethodsReturnEmptyCollections()
    {
        // Arrange
        var builder = new DependencyGraphBuilder();
        var graph = builder.Build();
        var dummyNode = new DependencyGraphNode("X", CreateVersion(1, 0));

        // Act & Assert
        Assert.Empty(graph.Nodes);
        Assert.Empty(graph.Edges);
        Assert.Empty(graph.ProjectLinks);
        Assert.Empty(graph.GetOutgoingEdges(dummyNode));
        Assert.Empty(graph.GetIncomingEdges(dummyNode));
        Assert.Empty(graph.GetLinksForProject("nonexistent"));
        Assert.Empty(graph.GetLinksForNode(dummyNode));
    }

    [Fact]
    public void FindNode_MissingNode_ReturnsNull()
    {
        // Arrange
        var builder = new DependencyGraphBuilder();
        builder.AddOrGetNode("A", CreateVersion(1, 0));
        var graph = builder.Build();

        // Act
        var result = graph.FindNode("nonexistent", CreateVersion(1, 0));

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void AddOrGetNode_DifferentIsMetadataAvailable_UpdatesNodeToMetadataUnavailable()
    {
        // Arrange
        var builder = new DependencyGraphBuilder();
        var version = CreateVersion(1, 0);

        // Act
        var first = builder.AddOrGetNode("A", version, isMetadataAvailable: true);
        var second = builder.AddOrGetNode("A", version, isMetadataAvailable: false);

        Assert.Equal(first, second);
        Assert.False(second.IsMetadataAvailable);
    }
}
