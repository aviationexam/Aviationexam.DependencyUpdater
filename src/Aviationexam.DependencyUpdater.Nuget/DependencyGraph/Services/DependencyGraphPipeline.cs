using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.DependencyGraph.Models;
using Aviationexam.DependencyUpdater.Nuget.Models;
using Aviationexam.DependencyUpdater.Nuget.Parsers;
using Aviationexam.DependencyUpdater.Nuget.Services;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Aviationexam.DependencyUpdater.Nuget.DependencyGraph.Services;

public sealed class DependencyGraphPipeline(
    NugetCsprojParser csprojParser,
    NugetFinder nugetFinder,
    DependencyGraphConstructor graphConstructor,
    DependencyGraphColorizer colorizer,
    ILogger<DependencyGraphPipeline> logger
)
{
    public async Task<DependencyGraph> BuildColoredGraphAsync(
        RepositoryConfig repositoryConfig,
        NugetUpdaterContext nugetUpdaterContext,
        IReadOnlyDictionary<NugetSource, NugetSourceRepository> sourceRepositories,
        CachingConfiguration cachingConfiguration,
        CancellationToken cancellationToken
    )
    {
        var allNugetFiles = nugetFinder.GetAllNugetFiles(repositoryConfig);
        var csprojFiles = allNugetFiles.Where(f => f.Type == ENugetFileType.Csproj).ToList();

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("Found {CsprojCount} csproj files for dependency graph", csprojFiles.Count);
        }

        var projects = new List<ProjectInfo>(csprojFiles.Count);

        foreach (var nugetFile in csprojFiles)
        {
            var projectName = Path.GetFileNameWithoutExtension(nugetFile.RelativePath);

            var packageReferences = csprojParser.Parse(
                repositoryConfig.TargetDirectoryPath, nugetFile
            ).ToList();

            var projectReferences = csprojParser.ParseProjectReferences(
                repositoryConfig.TargetDirectoryPath, nugetFile
            ).ToList();

            projects.Add(new ProjectInfo(
                projectName,
                nugetFile.RelativePath,
                packageReferences,
                projectReferences
            ));
        }

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("Parsed {ProjectCount} projects for dependency graph construction", projects.Count);
        }

        var graph = await graphConstructor.ConstructGraphAsync(
            nugetUpdaterContext,
            sourceRepositories,
            cachingConfiguration,
            cancellationToken
        );

        var coloredGraph = colorizer.ColorizeGraph(graph, projects);

        return coloredGraph;
    }
}
