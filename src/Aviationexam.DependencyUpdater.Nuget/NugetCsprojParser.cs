using Aviationexam.DependencyUpdater.Interfaces;
using Microsoft.Extensions.Logging;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Aviationexam.DependencyUpdater.Nuget;

public class NugetCsprojParser(
    IFileSystem fileSystem,
    ILogger<NugetCsprojParser> logger
)
{
    public IEnumerable<NugetDependency> Parse(
        string repositoryPath,
        NugetFile nugetFile,
        IReadOnlyCollection<NugetTargetFramework>? targetFrameworks = null
    )
    {
        var csprojFilePath = nugetFile.GetFullPath(repositoryPath);

        if (!fileSystem.Exists(csprojFilePath))
        {
            logger.LogError("File not found: {path}", csprojFilePath);
            yield break;
        }

        var baseDir = Path.GetDirectoryName(nugetFile.RelativePath);

        using var stream = fileSystem.FileOpen(csprojFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var doc = XDocument.Load(stream);

        targetFrameworks ??= ParseTargetFrameworks(doc).Distinct().ToList();

        foreach (var packageReference in doc.Descendants().Where(e => e.Name.LocalName == "PackageReference"))
        {
            var packageId = packageReference.Attribute("Include")?.Value;
            var versionValue = packageReference.Attribute("Version")?.Value;

            if (!string.IsNullOrEmpty(packageId))
            {
                VersionRange? version = null;
                if (!string.IsNullOrEmpty(versionValue))
                {
                    version = new VersionRange(new NuGetVersion(versionValue));
                }

                yield return new NugetDependency(
                    nugetFile,
                    new NugetPackageReference(packageId, version),
                    targetFrameworks
                );
            }
        }

        foreach (var import in doc.Descendants().Where(e => e.Name.LocalName == "Import"))
        {
            var importProject = import.Attribute("Project")?.Value.Replace('\\', Path.DirectorySeparatorChar);
            if (string.IsNullOrEmpty(importProject))
            {
                continue;
            }

            if (baseDir is not null)
            {
                importProject = Path.Combine(baseDir, importProject);
            }

            var importedFullPath = Path.GetFullPath(Path.Combine(repositoryPath, importProject));

            var importedPath = Path.GetRelativePath(repositoryPath, importedFullPath);

            if (!fileSystem.Exists(Path.Combine(repositoryPath, importedPath)))
            {
                logger.LogError("Imported file not found: {path}", importedPath);
                continue;
            }

            foreach (var importedDependency in Parse(repositoryPath, new NugetFile(importedPath, ENugetFileType.Targets), targetFrameworks))
            {
                yield return importedDependency;
            }
        }
    }

    private IEnumerable<NugetTargetFramework> ParseTargetFrameworks(XDocument doc)
    {
        var propertyGroups = doc.Descendants().Where(e => e.Name.LocalName == "PropertyGroup");

        foreach (var propertyGroup in propertyGroups)
        {
            var singleTargetFramework = propertyGroup.Elements().FirstOrDefault(e => e.Name.LocalName == "TargetFramework")?.Value;
            if (!string.IsNullOrWhiteSpace(singleTargetFramework))
            {
                yield return new NugetTargetFramework(singleTargetFramework.Trim());
            }

            var targetFrameworks = propertyGroup.Elements().FirstOrDefault(e => e.Name.LocalName == "TargetFrameworks")?.Value;
            if (!string.IsNullOrWhiteSpace(targetFrameworks))
            {
                foreach (var targetFramework in targetFrameworks.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    yield return new NugetTargetFramework(targetFramework.Trim());
                }
            }
        }
    }
}
