using Aviationexam.DependencyUpdater.Interfaces;
using Aviationexam.DependencyUpdater.Nuget.Models;
using Microsoft.Extensions.Logging;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Aviationexam.DependencyUpdater.Nuget.Parsers;

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
                importProject = Path.Join(baseDir, importProject);
            }

            var importedFullPath = Path.GetFullPath(Path.Join(repositoryPath, importProject));

            var importedPath = Path.GetRelativePath(repositoryPath, importedFullPath);

            if (!fileSystem.Exists(Path.Join(repositoryPath, importedPath)))
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
        var propertyGroups = doc.Descendants().Where(e => e.Name.LocalName == "PropertyGroup").ToList();

        var foundTargetFrameworkElement = false;
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
                    foundTargetFrameworkElement = true;
                    yield return new NugetTargetFramework(targetFramework.Trim());
                }
            }
        }

        if (foundTargetFrameworkElement)
        {
            yield break;
        }

        foreach (var propertyGroup in propertyGroups)
        {
            // Parse legacy .NET Framework format: <TargetFrameworkVersion>v4.7.2</TargetFrameworkVersion>
            var targetFrameworkVersion = propertyGroup.Elements().FirstOrDefault(e => e.Name.LocalName == "TargetFrameworkVersion")?.Value;
            if (!string.IsNullOrWhiteSpace(targetFrameworkVersion) && targetFrameworkVersion.StartsWith('v'))
            {
                // Map version to appropriate short TFM format (net461, net462, etc.)
                var netFrameworkTfm = MapFrameworkVersionToTfm(targetFrameworkVersion);
                if (netFrameworkTfm is not null)
                {
                    yield return new NugetTargetFramework(netFrameworkTfm);
                }
            }
        }
    }

    private string? MapFrameworkVersionToTfm(string frameworkVersion)
    {
        if (frameworkVersion.StartsWith('v'))
        {
            frameworkVersion = frameworkVersion[1..];
        }

        if (!Version.TryParse(frameworkVersion, out var parsedVersion))
        {
            return null;
        }

        IReadOnlyCollection<(Version, string)> knownVersions =
        [
            (Version.Parse("4.8.1"), "net481"),
            (Version.Parse("4.8"), "net48"),
            (Version.Parse("4.7.2"), "net472"),
            (Version.Parse("4.7.1"), "net471"),
            (Version.Parse("4.7"), "net47"),
            (Version.Parse("4.6.3"), "net463"),
            (Version.Parse("4.6.2"), "net462"),
            (Version.Parse("4.6.1"), "net461"),
            (Version.Parse("4.6.1"), "net452"),
            (Version.Parse("4.6.1"), "net451"),
            (Version.Parse("4.6.1"), "net45"),
            (Version.Parse("4.6.1"), "net403"),
            (Version.Parse("4.6.1"), "net40"),
            (Version.Parse("4.6.1"), "net35"),
            (Version.Parse("4.6.1"), "net20"),
            (Version.Parse("4.6.1"), "net11"),
        ];

        foreach (var (version, tfm) in knownVersions)
        {
            if (parsedVersion >= version)
            {
                return tfm;
            }
        }

        return null;
    }
}
