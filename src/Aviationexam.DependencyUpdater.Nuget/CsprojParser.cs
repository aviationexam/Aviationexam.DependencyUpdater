using System.Xml.Linq;
using System.IO;
using Aviationexam.DependencyUpdater.Interfaces;
using Microsoft.Extensions.Logging;
using NuGet.Versioning;
using System.Collections.Generic;
using System.Linq;

namespace Aviationexam.DependencyUpdater.Nuget;

public class CsprojParser(
    IFileSystem fileSystem,
    ILogger<CsprojParser> logger
)
{
    public IEnumerable<NugetDependency> Parse(NugetFile nugetFile)
    {
        var csprojFilePath = nugetFile.FullPath;

        if (!fileSystem.Exists(csprojFilePath))
        {
            logger.LogError("File not found: {path}", csprojFilePath);
            yield break;
        }

        var baseDir = Path.GetDirectoryName(csprojFilePath)!;

        using var stream = fileSystem.FileOpen(csprojFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var doc = XDocument.Load(stream);

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
                    new NugetPackageReference(packageId, version)
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

            var importedFullPath = Path.GetFullPath(Path.Combine(baseDir, importProject));

            if (!fileSystem.Exists(importedFullPath))
            {
                logger.LogError("Imported file not found: {path}", importedFullPath);
                continue;
            }

            var importedNugetFile = new NugetFile(importedFullPath, ENugetFileType.Targets);

            foreach (var importedDependency in Parse(importedNugetFile))
            {
                yield return importedDependency;
            }
        }
    }
}
