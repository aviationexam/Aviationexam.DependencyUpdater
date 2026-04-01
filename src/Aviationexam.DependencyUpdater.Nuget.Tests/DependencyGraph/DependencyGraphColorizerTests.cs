using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.DependencyGraph.Models;
using Aviationexam.DependencyUpdater.Nuget.DependencyGraph.Services;
using Aviationexam.DependencyUpdater.Nuget.Extensions;
using Aviationexam.DependencyUpdater.Nuget.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NuGet.Versioning;
using System.Linq;
using Xunit;

namespace Aviationexam.DependencyUpdater.Nuget.Tests.DependencyGraph;

public sealed class DependencyGraphColorizerTests
{
    private static readonly NugetTargetFramework Tfm = new("net9.0");

    private readonly DependencyGraphColorizer _colorizer = new(
        Substitute.For<ILogger<DependencyGraphColorizer>>()
    );

    private static PackageVersion V(int major, int minor, int patch = 0)
        => new NuGetVersion(major, minor, patch).MapToPackageVersion();

    private static NugetDependency MakePackageRef(string csprojPath, string packageName, int major, int minor, int patch = 0)
        => new(
            new NugetFile(csprojPath, ENugetFileType.Csproj),
            new NugetPackageReference(packageName, new VersionRange(new NuGetVersion(major, minor, patch)), null),
            [Tfm]
        );

    [Fact]
    public void ColorizeGraph_ExampleScenario_ProducesExpectedLinks()
    {
        // Arrange
        var builder = new DependencyGraphBuilder();
        var v1 = V(1, 0, 0);
        var nodeD = builder.AddOrGetNode("D", v1);
        var nodeE = builder.AddOrGetNode("E", v1);
        var nodeF = builder.AddOrGetNode("F", v1);
        builder.AddEdge(nodeD, nodeE, [Tfm]);
        builder.AddEdge(nodeD, nodeF, [Tfm]);
        var graph = builder.Build();

        var projectB = new ProjectInfo("B", "B/B.csproj",
            [MakePackageRef("B/B.csproj", "E", 1, 0, 0)],
            []);

        var projectA = new ProjectInfo("A", "A/A.csproj",
            [MakePackageRef("A/A.csproj", "D", 1, 0, 0)],
            [new ProjectReference("B", "../B/B.csproj", [Tfm])]);

        // Act
        var result = _colorizer.ColorizeGraph(graph, [projectA, projectB]);

        // Assert
        Assert.Equal(5, result.ProjectLinks.Count);

        Assert.Contains(result.ProjectLinks, l =>
            l.ProjectName == "B" && l.Node.PackageName == "E"
            && l.Nature == EDependencyLinkNature.Direct
            && l.TransitiveChain.SequenceEqual(new[] { "B" }));

        Assert.Contains(result.ProjectLinks, l =>
            l.ProjectName == "A" && l.Node.PackageName == "D"
            && l.Nature == EDependencyLinkNature.Direct
            && l.TransitiveChain.SequenceEqual(new[] { "A" }));

        Assert.Contains(result.ProjectLinks, l =>
            l.ProjectName == "A" && l.Node.PackageName == "E"
            && l.Nature == EDependencyLinkNature.Transitive
            && l.TransitiveChain.SequenceEqual(new[] { "A", "B" }));

        Assert.Contains(result.ProjectLinks, l =>
            l.ProjectName == "A" && l.Node.PackageName == "E"
            && l.Nature == EDependencyLinkNature.Transitive
            && l.TransitiveChain.SequenceEqual(new[] { "A" }));

        Assert.Contains(result.ProjectLinks, l =>
            l.ProjectName == "A" && l.Node.PackageName == "F"
            && l.Nature == EDependencyLinkNature.Transitive
            && l.TransitiveChain.SequenceEqual(new[] { "A" }));
    }

    [Fact]
    public void ColorizeGraph_DiamondDependency_ProducesMultipleTransitiveLinks()
    {
        // Arrange
        var builder = new DependencyGraphBuilder();
        var v1 = V(1, 0, 0);
        var nodeD = builder.AddOrGetNode("D", v1);
        var nodeE = builder.AddOrGetNode("E", v1);
        builder.AddEdge(nodeD, nodeE, [Tfm]);
        var graph = builder.Build();

        var projectB = new ProjectInfo("B", "B/B.csproj",
            [MakePackageRef("B/B.csproj", "E", 1, 0, 0)],
            []);

        var projectA = new ProjectInfo("A", "A/A.csproj",
            [MakePackageRef("A/A.csproj", "D", 1, 0, 0)],
            [new ProjectReference("B", "../B/B.csproj", [Tfm])]);

        // Act
        var result = _colorizer.ColorizeGraph(graph, [projectA, projectB]);

        // Assert
        var transitiveLinksAtoE = result.ProjectLinks
            .Where(l => l.ProjectName == "A" && l.Node.PackageName == "E" && l.Nature == EDependencyLinkNature.Transitive)
            .ToList();

        Assert.Equal(2, transitiveLinksAtoE.Count);

        Assert.Contains(transitiveLinksAtoE, l =>
            l.TransitiveChain.SequenceEqual(new[] { "A", "B" }));

        Assert.Contains(transitiveLinksAtoE, l =>
            l.TransitiveChain.SequenceEqual(new[] { "A" }));
    }

    [Fact]
    public void ColorizeGraph_MultiHopProjectReferences_ProducesTransitiveChain()
    {
        // Arrange
        var builder = new DependencyGraphBuilder();
        var v1 = V(1, 0, 0);
        builder.AddOrGetNode("E", v1);
        var graph = builder.Build();

        var projectC = new ProjectInfo("C", "C/C.csproj",
            [MakePackageRef("C/C.csproj", "E", 1, 0, 0)],
            []);

        var projectB = new ProjectInfo("B", "B/B.csproj",
            [],
            [new ProjectReference("C", "../C/C.csproj", [Tfm])]);

        var projectA = new ProjectInfo("A", "A/A.csproj",
            [],
            [new ProjectReference("B", "../B/B.csproj", [Tfm])]);

        // Act
        var result = _colorizer.ColorizeGraph(graph, [projectA, projectB, projectC]);

        // Assert
        Assert.Contains(result.ProjectLinks, l =>
            l.ProjectName == "A" && l.Node.PackageName == "E"
            && l.Nature == EDependencyLinkNature.Transitive
            && l.TransitiveChain.SequenceEqual(new[] { "A", "B", "C" }));

        Assert.Contains(result.ProjectLinks, l =>
            l.ProjectName == "B" && l.Node.PackageName == "E"
            && l.Nature == EDependencyLinkNature.Transitive
            && l.TransitiveChain.SequenceEqual(new[] { "B", "C" }));

        Assert.Contains(result.ProjectLinks, l =>
            l.ProjectName == "C" && l.Node.PackageName == "E"
            && l.Nature == EDependencyLinkNature.Direct
            && l.TransitiveChain.SequenceEqual(new[] { "C" }));
    }

    [Fact]
    public void ColorizeGraph_DirectAndTransitiveOverlap_ProducesBothLinks()
    {
        // Arrange
        var builder = new DependencyGraphBuilder();
        var v1 = V(1, 0, 0);
        builder.AddOrGetNode("E", v1);
        var graph = builder.Build();

        var projectB = new ProjectInfo("B", "B/B.csproj",
            [MakePackageRef("B/B.csproj", "E", 1, 0, 0)],
            []);

        var projectA = new ProjectInfo("A", "A/A.csproj",
            [MakePackageRef("A/A.csproj", "E", 1, 0, 0)],
            [new ProjectReference("B", "../B/B.csproj", [Tfm])]);

        // Act
        var result = _colorizer.ColorizeGraph(graph, [projectA, projectB]);

        // Assert
        var linksAtoE = result.ProjectLinks
            .Where(l => l.ProjectName == "A" && l.Node.PackageName == "E")
            .ToList();

        Assert.Contains(linksAtoE, l =>
            l.Nature == EDependencyLinkNature.Direct
            && l.TransitiveChain.SequenceEqual(new[] { "A" }));

        Assert.Contains(linksAtoE, l =>
            l.Nature == EDependencyLinkNature.Transitive
            && l.TransitiveChain.SequenceEqual(new[] { "A", "B" }));
    }

    [Fact]
    public void ColorizeGraph_EmptyProject_ProducesNoLinks()
    {
        // Arrange
        var builder = new DependencyGraphBuilder();
        builder.AddOrGetNode("E", V(1, 0, 0));
        var graph = builder.Build();

        var emptyProject = new ProjectInfo("Empty", "Empty/Empty.csproj", [], []);

        // Act
        var result = _colorizer.ColorizeGraph(graph, [emptyProject]);

        // Assert
        Assert.Empty(result.ProjectLinks);
    }

    [Fact]
    public void ColorizeGraph_TfmScopedEdges_TraversesAllEdges()
    {
        // Arrange
        var builder = new DependencyGraphBuilder();
        var v1 = V(1, 0, 0);
        var nodeD = builder.AddOrGetNode("D", v1);
        var nodeE = builder.AddOrGetNode("E", v1);
        var nodeF = builder.AddOrGetNode("F", v1);
        builder.AddEdge(nodeD, nodeE, [new NugetTargetFramework("net9.0")]);
        builder.AddEdge(nodeD, nodeF, [new NugetTargetFramework("net10.0")]);
        var graph = builder.Build();

        var projectA = new ProjectInfo("A", "A/A.csproj",
            [MakePackageRef("A/A.csproj", "D", 1, 0, 0)],
            []);

        // Act
        var result = _colorizer.ColorizeGraph(graph, [projectA]);

        // Assert
        Assert.Contains(result.ProjectLinks, l =>
            l.ProjectName == "A" && l.Node.PackageName == "D"
            && l.Nature == EDependencyLinkNature.Direct);

        Assert.Contains(result.ProjectLinks, l =>
            l.ProjectName == "A" && l.Node.PackageName == "E"
            && l.Nature == EDependencyLinkNature.Transitive);

        Assert.Contains(result.ProjectLinks, l =>
            l.ProjectName == "A" && l.Node.PackageName == "F"
            && l.Nature == EDependencyLinkNature.Transitive);
    }
}
