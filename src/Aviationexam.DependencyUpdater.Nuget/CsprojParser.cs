using Microsoft.Extensions.Logging;
using NuGet.Frameworks;
using NuGet.ProjectModel;
using System;
using System.Collections.Generic;
using System.IO;

namespace Aviationexam.DependencyUpdater.Nuget;

public class CsprojParser(
    ILogger<CsprojParser> logger
)
{
    public IReadOnlyCollection<NugetDependency> Parse(string csprojFilePath)
    {
        try
        {
            // Check if file exists
            if (!File.Exists(csprojFilePath))
            {
                logger.LogError("csproj file not found at {path}", csprojFilePath);

                return [];
            }

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
                    OriginalTargetFrameworks = new List<string> { "net9.0" },
                    TargetFrameworks = new List<ProjectRestoreMetadataFrameworkInfo>
                    {
                        new() { FrameworkName = new NuGetFramework(".NETCoreApp,Version=v9.0") },
                    },
                },
            };

            var dependencies = new List<NugetDependency>();

            // Extract PackageReferences
            foreach (var packageDependency in projectSpec.Dependencies)
            {
                dependencies.Add(new NugetDependency(packageDependency.Name, packageDependency.LibraryRange.VersionRange.OriginalString));
            }

            return dependencies;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error parsing csproj file {path}", csprojFilePath);

            return []; // Return an empty list on error
        }
    }
}
