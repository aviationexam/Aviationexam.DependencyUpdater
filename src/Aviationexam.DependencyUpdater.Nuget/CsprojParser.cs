using Aviationexam.DependencyUpdater.Interfaces;
using Microsoft.Build.Definition;
using Microsoft.Build.Evaluation;
using Microsoft.Extensions.Logging;
using NuGet.Versioning;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace Aviationexam.DependencyUpdater.Nuget;

public class CsprojParser(
    IFileSystem fileSystem,
    ILogger<CsprojParser> logger
)
{
    public IEnumerable<NugetDependency> Parse(NugetFile nugetFile)
    {
        var csprojFilePath = nugetFile.FullPath;

        // Check if file exists
        if (!fileSystem.Exists(csprojFilePath))
        {
            logger.LogError("csproj file not found at {path}", csprojFilePath);

            yield break;
        }

        using var stream = fileSystem.FileOpen(csprojFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var xml = XDocument.Load(stream);

        var csprojDir = Path.GetDirectoryName(csprojFilePath)!;
        var originalDir = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(csprojDir);

        using var projectCollection = new ProjectCollection();
        var project = Project.FromXmlReader(
            xml.CreateReader(),
            new ProjectOptions
            {
                GlobalProperties = new Dictionary<string, string> { ["DesignTimeBuild"] = "true" },
                ToolsVersion = null,
                SubToolsetVersion = null,
                ProjectCollection = projectCollection,
                LoadSettings = ProjectLoadSettings.Default,
                EvaluationContext = null,
                DirectoryCacheFactory = null,
                Interactive = false,
            }
        );

        Directory.SetCurrentDirectory(originalDir);

        foreach (var item in project.GetItems("PackageReference"))
        {
            if (item.IsImported)
            {
                continue;
            }

            var packageId = item.EvaluatedInclude;

            VersionRange? version = null;
            if (item.GetMetadataValue("Version") is { } versionValue && !string.IsNullOrEmpty(versionValue))
            {
                version = new VersionRange(new NuGetVersion(versionValue));
            }

            yield return new NugetDependency(nugetFile, new NugetPackageReference(packageId, version));
        }

        foreach (var import in project.Imports)
        {
            var importedPath = import.ImportedProject.FullPath;
            if (!fileSystem.Exists(importedPath))
            {
                logger.LogError("imported file not found at {path}", importedPath);

                continue;
            }

            foreach (var nugetDependency in Parse(new NugetFile(importedPath, ENugetFileType.Targets)))
            {
                yield return nugetDependency;
            }
        }
    }
}
