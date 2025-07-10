using Aviationexam.DependencyUpdater.Interfaces;
using Aviationexam.DependencyUpdater.Nuget.Models;
using Microsoft.Extensions.Logging;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml.Linq;

namespace Aviationexam.DependencyUpdater.Nuget.Parsers;

public class DotnetToolsParser(
    IFileSystem fileSystem,
    ILogger<DotnetToolsParser> logger
)
{
    public IEnumerable<NugetDependency> Parse(
        string repositoryPath,
        NugetFile nugetFile
    )
    {
        var csprojFilePath = nugetFile.GetFullPath(repositoryPath);

        if (!fileSystem.Exists(csprojFilePath))
        {
            logger.LogError("File not found: {path}", csprojFilePath);
            yield break;
        }

        using var stream = fileSystem.FileOpen(csprojFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);

        DotnetToolsManifest? manifest = null;
        try
        {
            manifest = JsonSerializer.Deserialize(
                stream,
                DotnetToolsManifestJsonContext.Default.DotnetToolsManifest
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to parse dotnet-tools.json: {path}", csprojFilePath);
        }

        if (manifest?.Tools is null)
        {
            yield break;
        }

        foreach (var (toolName, toolEntry) in manifest.Tools)
        {
            var package = new NugetPackageVersion(toolName, toolEntry.Version);
            yield return new NugetDependency(
                nugetFile,
                package,
                []
            );
        }
    }
}
