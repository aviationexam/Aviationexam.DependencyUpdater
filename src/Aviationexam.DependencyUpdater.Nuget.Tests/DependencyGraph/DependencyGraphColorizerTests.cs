using Aviationexam.DependencyUpdater.Nuget.DependencyGraph.Models;
using Aviationexam.DependencyUpdater.Nuget.DependencyGraph.Services;
using Aviationexam.DependencyUpdater.Nuget.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NuGet.Versioning;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Aviationexam.DependencyUpdater.Nuget.Tests.DependencyGraph;

public sealed class DependencyGraphColorizerTests
{
    private static readonly NugetTargetFramework Tfm = new("net9.0");

    private readonly DependencyGraphColorizer _colorizer = new(
        Substitute.For<ILogger<DependencyGraphColorizer>>()
    );

    private static NugetDependency MakePackageRef(string csprojPath, string packageName, int major, int minor, int patch = 0)
        => new(
            new NugetFile(csprojPath, ENugetFileType.Csproj),
            new NugetPackageReference(packageName, new VersionRange(new NuGetVersion(major, minor, patch)), null),
            [Tfm]
        );

    private static NugetFile MakeCsprojFile(string projectName)
        => new($"{projectName}/{projectName}.csproj", ENugetFileType.Csproj);

    private static ProjectReference MakeProjectRef(string definingProjectName, string referencedProjectName)
        => new(
            MakeCsprojFile(definingProjectName),
            referencedProjectName,
            $"../{referencedProjectName}/{referencedProjectName}.csproj",
            [Tfm]
        );

    [Fact]
    public void ColorizeGraph_ExampleScenario_ProducesExpectedLinks()
    {
        // Arrange
        var builder = new DependencyGraphBuilder();
        var nodeD = builder.AddOrGetNode(new NugetPackageVersion("D", "1.0.0"));
        var nodeE = builder.AddOrGetNode(new NugetPackageVersion("E", "1.0.0"));
        var nodeF = builder.AddOrGetNode(new NugetPackageVersion("F", "1.0.0"));
        builder.AddEdge(nodeD, nodeE, [Tfm]);
        builder.AddEdge(nodeD, nodeF, [Tfm]);
        var graph = builder.Build();

        IReadOnlyCollection<NugetDependency> packageDependencies =
        [
            MakePackageRef("A/A.csproj", "D", 1, 0, 0),
            MakePackageRef("B/B.csproj", "E", 1, 0, 0),
        ];

        IReadOnlyCollection<ProjectReference> projectReferences =
        [
            MakeProjectRef("A", "B"),
        ];

        // Act
        var result = _colorizer.ColorizeGraph(graph, packageDependencies, projectReferences);

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
        var nodeD = builder.AddOrGetNode(new NugetPackageVersion("D", "1.0.0"));
        var nodeE = builder.AddOrGetNode(new NugetPackageVersion("E", "1.0.0"));
        builder.AddEdge(nodeD, nodeE, [Tfm]);
        var graph = builder.Build();

        IReadOnlyCollection<NugetDependency> packageDependencies =
        [
            MakePackageRef("A/A.csproj", "D", 1, 0, 0),
            MakePackageRef("B/B.csproj", "E", 1, 0, 0),
        ];

        IReadOnlyCollection<ProjectReference> projectReferences =
        [
            MakeProjectRef("A", "B"),
        ];

        // Act
        var result = _colorizer.ColorizeGraph(graph, packageDependencies, projectReferences);

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
        builder.AddOrGetNode(new NugetPackageVersion("E", "1.0.0"));
        var graph = builder.Build();

        IReadOnlyCollection<NugetDependency> packageDependencies =
        [
            MakePackageRef("C/C.csproj", "E", 1, 0, 0),
        ];

        IReadOnlyCollection<ProjectReference> projectReferences =
        [
            MakeProjectRef("B", "C"),
            MakeProjectRef("A", "B"),
        ];

        // Act
        var result = _colorizer.ColorizeGraph(graph, packageDependencies, projectReferences);

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
        builder.AddOrGetNode(new NugetPackageVersion("E", "1.0.0"));
        var graph = builder.Build();

        IReadOnlyCollection<NugetDependency> packageDependencies =
        [
            MakePackageRef("A/A.csproj", "E", 1, 0, 0),
            MakePackageRef("B/B.csproj", "E", 1, 0, 0),
        ];

        IReadOnlyCollection<ProjectReference> projectReferences =
        [
            MakeProjectRef("A", "B"),
        ];

        // Act
        var result = _colorizer.ColorizeGraph(graph, packageDependencies, projectReferences);

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
        builder.AddOrGetNode(new NugetPackageVersion("E", "1.0.0"));
        var graph = builder.Build();

        // Act
        var result = _colorizer.ColorizeGraph(graph, [], []);

        // Assert
        Assert.Empty(result.ProjectLinks);
    }

    [Fact]
    public void ColorizeGraph_TfmScopedEdges_TraversesAllEdges()
    {
        // Arrange
        var builder = new DependencyGraphBuilder();
        var nodeD = builder.AddOrGetNode(new NugetPackageVersion("D", "1.0.0"));
        var nodeE = builder.AddOrGetNode(new NugetPackageVersion("E", "1.0.0"));
        var nodeF = builder.AddOrGetNode(new NugetPackageVersion("F", "1.0.0"));
        builder.AddEdge(nodeD, nodeE, [new NugetTargetFramework("net9.0")]);
        builder.AddEdge(nodeD, nodeF, [new NugetTargetFramework("net10.0")]);
        var graph = builder.Build();

        IReadOnlyCollection<NugetDependency> packageDependencies =
        [
            MakePackageRef("A/A.csproj", "D", 1, 0, 0),
        ];

        // Act
        var result = _colorizer.ColorizeGraph(graph, packageDependencies, []);

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
