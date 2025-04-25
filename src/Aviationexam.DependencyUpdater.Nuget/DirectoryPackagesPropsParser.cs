using Aviationexam.DependencyUpdater.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Aviationexam.DependencyUpdater.Nuget;

public class DirectoryPackagesPropsParser(
    IFileSystem fileSystem,
    ILogger<DirectoryPackagesPropsParser> logger
)
{
    public IEnumerable<NugetDependency> Parse(NugetFile nugetFile)
    {
        var csprojFilePath = nugetFile.FullPath;

        // Check if file exists
        if (!fileSystem.Exists(csprojFilePath))
        {
            logger.LogError("csproj file not found at {path}", csprojFilePath);

            return [];
        }

        using var stream = fileSystem.FileOpen(nugetFile.FullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var doc = XDocument.Load(stream);

        return doc
            .Descendants()
            .Where(e => e.Name.LocalName == "PackageVersion")
            .Select(x => new
            {
                Include = x.Attribute("Include")?.Value,
                Version = x.Attribute("Version")?.Value,
            })
            .Where(x => x.Include is not null && x.Version is not null)
            .Select(x => new NugetDependency(
                nugetFile,
                new NugetPackageVersion(
                    x.Include!,
                    x.Version!
                )
            ));
    }
}
