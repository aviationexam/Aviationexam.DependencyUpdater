using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Interfaces;
using Aviationexam.DependencyUpdater.Nuget.Extensions;
using Aviationexam.DependencyUpdater.Nuget.Helpers;
using Aviationexam.DependencyUpdater.Nuget.Models;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using ZLinq;

namespace Aviationexam.DependencyUpdater.Nuget.Writers;

public sealed class NugetDirectoryPackagesPropsVersionWriter(
    IFileSystem fileSystem
)
{

    public async Task<ESetVersion> TrySetVersionAsync(
        NugetUpdateCandidate nugetUpdateCandidate,
        string fullPath,
        IDictionary<string, PackageVersion> groupPackageVersions,
        CancellationToken cancellationToken
    )
    {
        var packageName = nugetUpdateCandidate.NugetDependency.NugetPackage.GetPackageName();

        await using var fileStream = fileSystem.FileOpen(fullPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

        var doc = await XDocument.LoadAsync(fileStream, LoadOptions.PreserveWhitespace, cancellationToken);

        // Find the matching PackageVersion element, considering target frameworks
        var versionAttribute = FindMatchingPackageVersionAttribute(
            doc,
            packageName,
            nugetUpdateCandidate.NugetDependency.TargetFrameworks
        );

        if (versionAttribute is null)
        {
            return ESetVersion.VersionNotSet;
        }

        versionAttribute.Value = nugetUpdateCandidate.PossiblePackageVersion.PackageVersion.GetSerializedVersion();

        fileStream.Seek(0, SeekOrigin.Begin);
        await using var xmlWriter = XmlWriter.Create(fileStream, new XmlWriterSettings
        {
            Async = true,
            CloseOutput = false,
            ConformanceLevel = ConformanceLevel.Auto,
            DoNotEscapeUriAttributes = true,
            Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            Indent = false,
            NamespaceHandling = NamespaceHandling.Default,
            NewLineHandling = NewLineHandling.None,
            NewLineOnAttributes = false,
            OmitXmlDeclaration = true,
            WriteEndDocumentOnClose = false,
        });

        await doc.SaveAsync(xmlWriter, cancellationToken);
        fileStream.SetLength(fileStream.Position);

        if (groupPackageVersions.ContainsKey(packageName))
        {
            groupPackageVersions[packageName] = nugetUpdateCandidate.PossiblePackageVersion.PackageVersion;
        }

        return ESetVersion.VersionSet;
    }

    private XAttribute? FindMatchingPackageVersionAttribute(
        XDocument doc,
        string packageName,
        IReadOnlyCollection<NugetTargetFramework> targetFrameworks
    )
    {
        var packageVersionElements = doc
            .Descendants()
            .AsValueEnumerable()
            .Where(e => e.Name.LocalName == "PackageVersion")
            .Where(e => e.Attribute("Include")?.Value == packageName)
            .ToList();

        if (packageVersionElements.Count == 0)
        {
            return null;
        }

        // Multiple matches - need to find the one matching the target framework
        // Priority:
        // 1. PackageVersion element with matching Condition
        // 2. PackageVersion in ItemGroup with matching Condition
        // 3. PackageVersion without Condition (unconditional)

        var targetFrameworkNames = targetFrameworks.AsValueEnumerable().Select(tf => tf.TargetFramework).ToHashSet();

        // First, try to find a conditional match for the target frameworks
        foreach (var element in packageVersionElements)
        {
            var condition = element.GetConditionIncludingParent();
            if (TargetFrameworkConditionHelper.TryExtractTargetFramework(condition, out var conditionalTfm) &&
                targetFrameworkNames.Contains(conditionalTfm))
            {
                return element.Attribute("Version");
            }
        }

        // If no conditional match found, return the first unconditional one
        foreach (var element in packageVersionElements)
        {
            var condition = element.GetConditionIncludingParent();
            if (string.IsNullOrWhiteSpace(condition))
            {
                return element.Attribute("Version");
            }
        }

        // Fallback: return the first match (shouldn't happen, but better than null)
        return packageVersionElements[0].Attribute("Version");
    }
}
