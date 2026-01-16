using Aviationexam.DependencyUpdater.Interfaces;
using Aviationexam.DependencyUpdater.Nuget.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Aviationexam.DependencyUpdater.Nuget.Parsers;

public class DotnetToolsParser(
    IFileSystem fileSystem,
    ILogger<DotnetToolsParser> logger
)
{
    public IEnumerable<NugetDependency> Parse(
        string repositoryPath,
        NugetFile nugetFile,
        IReadOnlyCollection<NugetTargetFramework> targetFrameworks
    )
    {
        var dotnetToolFullPath = nugetFile.GetFullPath(repositoryPath);

        if (!fileSystem.Exists(dotnetToolFullPath))
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                logger.LogError("File not found: {path}", dotnetToolFullPath);
            }
            yield break;
        }

        using var stream = fileSystem.FileOpen(dotnetToolFullPath, FileMode.Open, FileAccess.Read, FileShare.Read);

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
            if (logger.IsEnabled(LogLevel.Error))
            {
                logger.LogError(ex, "Failed to parse dotnet-tools.json: {path}", dotnetToolFullPath);
            }
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
                targetFrameworks
            );
        }
    }
}
