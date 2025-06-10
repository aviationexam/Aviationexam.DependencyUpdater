using Aviationexam.DependencyUpdater.Interfaces;
using Aviationexam.DependencyUpdater.Nuget.Models;
using Microsoft.Extensions.Logging;
using NuGet.Versioning;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Aviationexam.DependencyUpdater.Nuget.Parsers;

public class NugetDirectoryPackagesPropsParser(
    IFileSystem fileSystem,
    ILogger<NugetDirectoryPackagesPropsParser> logger
)
{
    public IEnumerable<NugetDependency> Parse(
        string repositoryPath,
        NugetFile nugetFile,
        IReadOnlyCollection<NugetTargetFramework> targetFrameworks
    )
    {
        var directoryPackagesPropsFilePath = nugetFile.GetFullPath(repositoryPath);

        // Check if file exists
        if (!fileSystem.Exists(directoryPackagesPropsFilePath))
        {
            logger.LogError("csproj file not found at {path}", directoryPackagesPropsFilePath);

            return [];
        }

        using var stream = fileSystem.FileOpen(directoryPackagesPropsFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
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
                    new NuGetVersion(x.Version!)
                ),
                targetFrameworks
            ));
    }
}
