using Aviationexam.DependencyUpdater.Nuget.DependencyGraph.Models;
using Aviationexam.DependencyUpdater.Nuget.Extensions;
using Aviationexam.DependencyUpdater.Nuget.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Aviationexam.DependencyUpdater.Nuget.DependencyGraph.Services;

public sealed class DependencyGraphColorizer(
    ILogger<DependencyGraphColorizer> logger
)
{
    public DependencyGraph ColorizeGraph(
        DependencyGraph graph,
        IReadOnlyCollection<NugetDependency> packageDependencies,
        IReadOnlyCollection<ProjectReference> projectReferences
    )
    {
        var packageDependenciesByProject = BuildPackageDependencyMap(packageDependencies);
        var projectReferencesByProject = BuildProjectReferenceMap(projectReferences);
        var allProjectNames = packageDependenciesByProject.Keys
            .Concat(projectReferencesByProject.Keys)
            .Distinct()
            .ToList();

        var allLinks = new List<ProjectDependencyLink>();
        var seedLinks = new List<ProjectDependencyLink>();

        AddDirectLinks(graph, packageDependenciesByProject, allLinks, seedLinks);
        AddTransitiveProjectReferenceLinks(graph, allProjectNames, packageDependenciesByProject, projectReferencesByProject, allLinks, seedLinks);
        AddTransitivePackageGraphLinks(graph, seedLinks, allLinks);

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Colorized dependency graph with {ProjectCount} projects and {LinkCount} project links",
                allProjectNames.Count,
                allLinks.Count
            );
        }

        return RebuildGraphWithProjectLinks(graph, allLinks);
    }

    private static Dictionary<string, IReadOnlyCollection<NugetDependency>> BuildPackageDependencyMap(IReadOnlyCollection<NugetDependency> packageDependencies)
        => packageDependencies
            .GroupBy(dependency => GetProjectName(dependency.NugetFile))
            .ToDictionary(group => group.Key, group => (IReadOnlyCollection<NugetDependency>) group.ToList());

    private static Dictionary<string, IReadOnlyCollection<ProjectReference>> BuildProjectReferenceMap(IReadOnlyCollection<ProjectReference> projectReferences)
        => projectReferences
            .GroupBy(projectReference => GetProjectName(projectReference.NugetFile))
            .ToDictionary(group => group.Key, group => (IReadOnlyCollection<ProjectReference>) group.ToList());

    private static string GetProjectName(NugetFile nugetFile)
        => Path.GetFileNameWithoutExtension(nugetFile.RelativePath);

    private static void AddDirectLinks(
        DependencyGraph graph,
        IReadOnlyDictionary<string, IReadOnlyCollection<NugetDependency>> packageDependenciesByProject,
        ICollection<ProjectDependencyLink> allLinks,
        ICollection<ProjectDependencyLink> seedLinks
    )
    {
        foreach (var (projectName, packageDependencies) in packageDependenciesByProject)
        {
            foreach (var packageReference in packageDependencies)
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
                    projectName,
                    node,
                    EDependencyLinkNature.Direct,
                    [projectName]
                );

                allLinks.Add(link);
                seedLinks.Add(link);
            }
        }
    }

    private static void AddTransitiveProjectReferenceLinks(
        DependencyGraph graph,
        IReadOnlyCollection<string> allProjectNames,
        IReadOnlyDictionary<string, IReadOnlyCollection<NugetDependency>> packageDependenciesByProject,
        IReadOnlyDictionary<string, IReadOnlyCollection<ProjectReference>> projectReferencesByProject,
        ICollection<ProjectDependencyLink> allLinks,
        ICollection<ProjectDependencyLink> seedLinks
    )
    {
        foreach (var projectName in allProjectNames)
        {
            var stack = new Stack<(string ProjectName, List<string> Chain)>();
            var rootChain = new List<string> { projectName };

            if (!projectReferencesByProject.TryGetValue(projectName, out var rootProjectReferences))
            {
                continue;
            }

            foreach (var projectReference in rootProjectReferences)
            {
                var chain = new List<string>(rootChain)
                {
                    projectReference.ProjectName,
                };

                stack.Push((projectReference.ProjectName, chain));
            }

            while (stack.TryPop(out var item))
            {
                var (currentProjectName, chain) = item;

                AddLinksForProjectPackageReferences(
                    graph,
                    projectName,
                    currentProjectName,
                    chain,
                    packageDependenciesByProject,
                    allLinks,
                    seedLinks
                );

                if (!projectReferencesByProject.TryGetValue(currentProjectName, out var currentProjectReferences))
                {
                    continue;
                }

                foreach (var projectReference in currentProjectReferences)
                {
                    if (chain.Contains(projectReference.ProjectName))
                    {
                        continue;
                    }

                    var nextChain = new List<string>(chain)
                    {
                        projectReference.ProjectName,
                    };

                    stack.Push((projectReference.ProjectName, nextChain));
                }
            }
        }
    }

    private static void AddLinksForProjectPackageReferences(
        DependencyGraph graph,
        string rootProjectName,
        string dependencyProjectName,
        IReadOnlyList<string> chain,
        IReadOnlyDictionary<string, IReadOnlyCollection<NugetDependency>> packageDependenciesByProject,
        ICollection<ProjectDependencyLink> allLinks,
        ICollection<ProjectDependencyLink> seedLinks
    )
    {
        if (!packageDependenciesByProject.TryGetValue(dependencyProjectName, out var packageDependencies))
        {
            return;
        }

        foreach (var packageReference in packageDependencies)
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
            builder.AddOrGetNode(node);
        }

        foreach (var edge in graph.Edges)
        {
            var from = builder.AddOrGetNode(edge.From);
            var to = builder.AddOrGetNode(edge.To);
            builder.AddEdge(from, to, edge.TargetFrameworks);
        }

        foreach (var link in links)
        {
            var node = builder.AddOrGetNode(link.Node);
            builder.AddProjectLink(new ProjectDependencyLink(link.ProjectName, node, link.Nature, link.TransitiveChain));
        }

        return builder.Build();
    }
}
