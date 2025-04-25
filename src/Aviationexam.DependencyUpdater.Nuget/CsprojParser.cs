using Aviationexam.DependencyUpdater.Interfaces;
using Microsoft.Extensions.Logging;
using NuGet.Frameworks;
using NuGet.LibraryModel;
using NuGet.ProjectModel;
using System;
using System.Collections.Generic;
using System.IO;

namespace Aviationexam.DependencyUpdater.Nuget;

public class CsprojParser(
    IFileSystem filesystem,
    ILogger<CsprojParser> logger
)
{
    public IEnumerable<NugetDependency> Parse(NugetFile nugetFile)
    {
        var csprojFilePath = nugetFile.FullPath;

        // Check if file exists
        if (!filesystem.Exists(csprojFilePath))
        {
            logger.LogError("csproj file not found at {path}", csprojFilePath);

            yield break;
        }

        IList<LibraryDependency>? dependencies = null;
        try
        {
            // Create a project spec reader
            var projectSpec = new PackageSpec
            {
                FilePath = csprojFilePath,
                RestoreMetadata = new ProjectRestoreMetadata
                {
                    ProjectPath = csprojFilePath,
                    ProjectName = Path.GetFileNameWithoutExtension(csprojFilePath),
                    ProjectUniqueName = csprojFilePath,
                    ProjectStyle = ProjectStyle.PackageReference,
                    OutputPath = Path.Combine(Path.GetDirectoryName(csprojFilePath), "obj"),
                    OriginalTargetFrameworks = new List<string> { "net8.0" },
                    TargetFrameworks = new List<ProjectRestoreMetadataFrameworkInfo>
                    {
                        new() { FrameworkName = new NuGetFramework(".NETCoreApp,Version=v8.0") },
                    },
                },
            };

            var t=projectSpec.TargetFrameworks;

            dependencies = projectSpec.Dependencies;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error parsing csproj file {path}", csprojFilePath);
        }

        if (dependencies is null)
        {
            logger.LogWarning("Error parsing csproj dependencies {path}", csprojFilePath);

            yield break;
        }

        // Extract PackageReferences
        foreach (var packageDependency in dependencies)
        {
            yield return new NugetDependency(
                nugetFile,
                new NugetPackageReference(
                    packageDependency.Name, packageDependency.LibraryRange.VersionRange
                )
            );
        }
    }
}
