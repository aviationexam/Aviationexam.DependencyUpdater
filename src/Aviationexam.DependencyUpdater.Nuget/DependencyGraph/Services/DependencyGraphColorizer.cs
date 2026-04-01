using Aviationexam.DependencyUpdater.Nuget.DependencyGraph.Models;
using Aviationexam.DependencyUpdater.Nuget.Extensions;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Aviationexam.DependencyUpdater.Nuget.DependencyGraph.Services;

public sealed class DependencyGraphColorizer(
    ILogger<DependencyGraphColorizer> logger
)
{
    public DependencyGraph ColorizeGraph(
        DependencyGraph graph,
        IReadOnlyCollection<ProjectInfo> projects
    )
    {
        var projectByName = BuildProjectMap(projects);
        var allLinks = new List<ProjectDependencyLink>();
        var seedLinks = new List<ProjectDependencyLink>();

        AddDirectLinks(graph, projects, allLinks, seedLinks);
        AddTransitiveProjectReferenceLinks(graph, projects, projectByName, allLinks, seedLinks);
        AddTransitivePackageGraphLinks(graph, seedLinks, allLinks);

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Colorized dependency graph with {ProjectCount} projects and {LinkCount} project links",
                projects.Count,
                allLinks.Count
            );
        }

        return RebuildGraphWithProjectLinks(graph, allLinks);
    }

    private static Dictionary<string, ProjectInfo> BuildProjectMap(IReadOnlyCollection<ProjectInfo> projects)
    {
        var result = new Dictionary<string, ProjectInfo>();

        foreach (var project in projects)
        {
            if (!result.ContainsKey(project.ProjectName))
            {
                result[project.ProjectName] = project;
            }
        }

        return result;
    }

    private static void AddDirectLinks(
        DependencyGraph graph,
        IReadOnlyCollection<ProjectInfo> projects,
        ICollection<ProjectDependencyLink> allLinks,
        ICollection<ProjectDependencyLink> seedLinks
    )
    {
        foreach (var project in projects)
        {
            foreach (var packageReference in project.PackageReferences)
            {
                var packageName = packageReference.NugetPackage.GetPackageName();
                var packageVersion = packageReference.NugetPackage.GetVersion();

                if (packageVersion is null)
                {
                    continue;
                }

                var node = graph.FindNode(packageName, packageVersion);

                if (node is null)
                {
                    continue;
                }

                var link = new ProjectDependencyLink(
                    project.ProjectName,
                    node,
                    EDependencyLinkNature.Direct,
                    [project.ProjectName]
                );

                allLinks.Add(link);
                seedLinks.Add(link);
            }
        }
    }

    private static void AddTransitiveProjectReferenceLinks(
        DependencyGraph graph,
        IReadOnlyCollection<ProjectInfo> projects,
        IReadOnlyDictionary<string, ProjectInfo> projectByName,
        ICollection<ProjectDependencyLink> allLinks,
        ICollection<ProjectDependencyLink> seedLinks
    )
    {
        foreach (var project in projects)
        {
            var stack = new Stack<(ProjectInfo Project, List<string> Chain)>();
            var rootChain = new List<string> { project.ProjectName };

            foreach (var projectReference in project.ProjectReferences)
            {
                if (!projectByName.TryGetValue(projectReference.ProjectName, out var referencedProject))
                {
                    continue;
                }

                var chain = new List<string>(rootChain)
                {
                    referencedProject.ProjectName,
                };

                stack.Push((referencedProject, chain));
            }

            while (stack.TryPop(out var item))
            {
                var (currentProject, chain) = item;

                AddLinksForProjectPackageReferences(
                    graph,
                    project.ProjectName,
                    currentProject,
                    chain,
                    allLinks,
                    seedLinks
                );

                foreach (var projectReference in currentProject.ProjectReferences)
                {
                    if (!projectByName.TryGetValue(projectReference.ProjectName, out var nextProject))
                    {
                        continue;
                    }

                    if (chain.Contains(nextProject.ProjectName))
                    {
                        continue;
                    }

                    var nextChain = new List<string>(chain)
                    {
                        nextProject.ProjectName,
                    };

                    stack.Push((nextProject, nextChain));
                }
            }
        }
    }

    private static void AddLinksForProjectPackageReferences(
        DependencyGraph graph,
        string rootProjectName,
        ProjectInfo dependencyProject,
        IReadOnlyList<string> chain,
        ICollection<ProjectDependencyLink> allLinks,
        ICollection<ProjectDependencyLink> seedLinks
    )
    {
        foreach (var packageReference in dependencyProject.PackageReferences)
        {
            var packageName = packageReference.NugetPackage.GetPackageName();
            var packageVersion = packageReference.NugetPackage.GetVersion();

            if (packageVersion is null)
            {
                continue;
            }

            var node = graph.FindNode(packageName, packageVersion);

            if (node is null)
            {
                continue;
            }

            var link = new ProjectDependencyLink(
                rootProjectName,
                node,
                EDependencyLinkNature.Transitive,
                [.. chain]
            );

            allLinks.Add(link);
            seedLinks.Add(link);
        }
    }

    private static void AddTransitivePackageGraphLinks(
        DependencyGraph graph,
        IReadOnlyCollection<ProjectDependencyLink> seedLinks,
        ICollection<ProjectDependencyLink> allLinks
    )
    {
        foreach (var seedLink in seedLinks)
        {
            var stack = new Stack<(DependencyGraphNode Node, HashSet<DependencyGraphNode> PathVisited)>();
            stack.Push((seedLink.Node, [seedLink.Node]));

            while (stack.TryPop(out var item))
            {
                foreach (var edge in graph.GetOutgoingEdges(item.Node))
                {
                    var target = edge.To;

                    if (item.PathVisited.Contains(target))
                    {
                        continue;
                    }

                    allLinks.Add(new ProjectDependencyLink(
                        seedLink.ProjectName,
                        target,
                        EDependencyLinkNature.Transitive,
                        [.. seedLink.TransitiveChain]
                    ));

                    var nextVisited = new HashSet<DependencyGraphNode>(item.PathVisited)
                    {
                        target,
                    };

                    stack.Push((target, nextVisited));
                }
            }
        }
    }

    private static DependencyGraph RebuildGraphWithProjectLinks(
        DependencyGraph graph,
        IReadOnlyCollection<ProjectDependencyLink> links
    )
    {
        var builder = new DependencyGraphBuilder();

        foreach (var node in graph.Nodes.Values)
        {
            builder.AddOrGetNode(node.PackageName, node.Version, node.IsMetadataAvailable);
        }

        foreach (var edge in graph.Edges)
        {
            var from = builder.AddOrGetNode(edge.From.PackageName, edge.From.Version, edge.From.IsMetadataAvailable);
            var to = builder.AddOrGetNode(edge.To.PackageName, edge.To.Version, edge.To.IsMetadataAvailable);
            builder.AddEdge(from, to, edge.TargetFrameworks);
        }

        foreach (var link in links)
        {
            var node = builder.AddOrGetNode(link.Node.PackageName, link.Node.Version, link.Node.IsMetadataAvailable);
            builder.AddProjectLink(new ProjectDependencyLink(link.ProjectName, node, link.Nature, link.TransitiveChain));
        }

        return builder.Build();
    }
}
