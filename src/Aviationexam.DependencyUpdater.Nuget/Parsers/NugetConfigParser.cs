using Aviationexam.DependencyUpdater.Interfaces;
using Aviationexam.DependencyUpdater.Nuget.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using ZLinq;

namespace Aviationexam.DependencyUpdater.Nuget.Parsers;

public class NugetConfigParser(
    IFileSystem fileSystem,
    ILogger<NugetConfigParser> logger
)
{
    public IEnumerable<NugetSource> Parse(
        string repositoryPath,
        NugetFile nugetFile
    )
    {
        var configFilePath = nugetFile.GetFullPath(repositoryPath);

        if (!fileSystem.Exists(configFilePath))
        {
            logger.LogError("NuGet config file not found: {path}", configFilePath);

            yield break;
        }

        using var stream = fileSystem.FileOpen(configFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var doc = XDocument.Load(stream);

        var packageSourcesElement = doc.Descendants()
            .AsValueEnumerable()
            .FirstOrDefault(e => e.Name.LocalName == "packageSources");

        var packageSourceMappingElement = doc.Descendants()
            .AsValueEnumerable()
            .FirstOrDefault(e => e.Name.LocalName == "packageSourceMapping");

        if (packageSourcesElement is null)
        {
            logger.LogWarning("No <packageSources> section found in: {path}", configFilePath);

            yield break;
        }

        // Parse package source mappings first (optional)
        var sourceMappings = new Dictionary<string, ImmutableArray<NugetPackageSourceMap>>();

        if (packageSourceMappingElement is not null)
        {
            foreach (var packageSourceElement in packageSourceMappingElement.Elements()
                         .Where(e => e.Name.LocalName == "packageSource"))
            {
                var sourceKey = packageSourceElement.Attribute("key")?.Value;

                if (string.IsNullOrWhiteSpace(sourceKey))
                {
                    continue;
                }

                sourceMappings[sourceKey] =
                [
                    .. packageSourceElement.Elements()
                        .Where(e => e.Name.LocalName == "package")
                        .Select(x => x.Attribute("pattern")?.Value)
                        .Where(x => x is not null)
                        .Select(x => new NugetPackageSourceMap(x!))
                ];
            }
        }

        foreach (var addElement in packageSourcesElement.Elements()
                     .Where(e => e.Name.LocalName == "add"))
        {
            var name = addElement.Attribute("key")?.Value;
            var value = addElement.Attribute("value")?.Value;
            var protocolVersionString = addElement.Attribute("protocolVersion")?.Value;

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            var version = protocolVersionString switch
            {
                "3" => NugetSourceVersion.V3,
                "2" => NugetSourceVersion.V2,
                _ => NugetSourceVersion.Unknown,
            };

            yield return new NugetSource(
                name,
                value,
                version,
                sourceMappings.TryGetValue(name, out var mappings)
                    ? mappings
                    : []
            );
        }
    }
}
