using Aviationexam.DependencyUpdater.Interfaces;
using Aviationexam.DependencyUpdater.Nuget.Models;
using Microsoft.Extensions.Logging;
using NuGet.Versioning;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using ZLinq;

namespace Aviationexam.DependencyUpdater.Nuget.Parsers;

public partial class NugetDirectoryPackagesPropsParser(
    IFileSystem fileSystem,
    ILogger<NugetDirectoryPackagesPropsParser> logger
)
{
    // Matches Condition="'$(TargetFramework)' == 'net9.0'" or Condition="'$(TargetFramework)'=='net9.0'" (with or without spaces)
    [GeneratedRegex(@"\'\$\(TargetFramework\)\'\s*==\s*\'(?<tfm>[^\']+)\'", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 100)]
    private static partial Regex TargetFrameworkConditionRegex();

    public IEnumerable<NugetDependency> Parse(
        string repositoryPath,
        NugetFile nugetFile,
        IReadOnlyDictionary<string, IReadOnlyCollection<NugetTargetFramework>> packagesTargetFrameworks,
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
            .AsValueEnumerable()
            .Where(e => e.Name.LocalName == "PackageVersion")
            .Select(x => new
            {
                Include = x.Attribute("Include")?.Value,
                Version = x.Attribute("Version")?.Value,
                Condition = x.Attribute("Condition")?.Value ?? x.Parent?.Attribute("Condition")?.Value,
            })
            .Where(x => x.Include is not null && x.Version is not null)
            .Select(x => new NugetDependency(
                nugetFile,
                new NugetPackageVersion(
                    x.Include!,
                    new NuGetVersion(x.Version!)
                ),
                GetEffectiveTargetFrameworks(
                    x.Condition,
                    x.Include!,
                    packagesTargetFrameworks,
                    targetFrameworks
                )
            ))
            .ToList();
    }

    private IReadOnlyCollection<NugetTargetFramework> GetEffectiveTargetFrameworks(
        string? condition,
        string packageName,
        IReadOnlyDictionary<string, IReadOnlyCollection<NugetTargetFramework>> packagesTargetFrameworks,
        IReadOnlyCollection<NugetTargetFramework> defaultTargetFrameworks
    )
    {
        // If there's a condition, try to extract the target framework
        if (!string.IsNullOrWhiteSpace(condition))
        {
            var match = TargetFrameworkConditionRegex().Match(condition);
            if (match.Success)
            {
                var conditionalTargetFramework = match.Groups["tfm"].Value;
                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug(
                        "Found conditional target framework {TargetFramework} for package {PackageName}",
                        conditionalTargetFramework,
                        packageName
                    );
                }
                return [new NugetTargetFramework(conditionalTargetFramework)];
            }
            
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning(
                    "Unable to parse condition '{Condition}' for package {PackageName}",
                    condition,
                    packageName
                );
            }
        }

        // Fall back to default behavior
        if (packagesTargetFrameworks.TryGetValue(packageName, out var packageTargetFrameworks))
        {
            return packageTargetFrameworks.AsValueEnumerable().Union(defaultTargetFrameworks).ToList();
        }

        return defaultTargetFrameworks;
    }
}
