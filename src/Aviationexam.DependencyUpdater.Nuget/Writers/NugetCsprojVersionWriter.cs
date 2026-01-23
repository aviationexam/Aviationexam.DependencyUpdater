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

public sealed class NugetCsprojVersionWriter(
    IFileSystem fileSystem
)
{
    public async Task<ESetVersion> TrySetVersionAsync(
        NugetUpdateCandidate nugetUpdateCandidate,
        string fullPath,
        CurrentPackageVersions groupPackageVersions,
        CancellationToken cancellationToken
    )
    {
        var packageName = nugetUpdateCandidate.NugetDependency.NugetDependency.NugetPackage.GetPackageName();

        await using var fileStream = fileSystem.FileOpen(fullPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

        var doc = await XDocument.LoadAsync(fileStream, LoadOptions.PreserveWhitespace, cancellationToken);

        var versionAttribute = FindMatchingPackageReferenceAttribute(
            doc,
            packageName,
            nugetUpdateCandidate.NugetDependency.NugetDependency.TargetFrameworks
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

        groupPackageVersions.UpdateVersionForTargetFrameworks(
            packageName,
            nugetUpdateCandidate.NugetDependency.NugetDependency.TargetFrameworks,
            nugetUpdateCandidate.PossiblePackageVersion.PackageVersion
        );

        return ESetVersion.VersionSet;
    }

    private XAttribute? FindMatchingPackageReferenceAttribute(
        XDocument doc,
        string packageName,
        IReadOnlyCollection<NugetTargetFramework> targetFrameworks
    )
    {
        var packageReferenceElements = doc
            .Descendants()
            .AsValueEnumerable()
            .Where(e => e.Name.LocalName == "PackageReference")
            .Where(e => e.Attribute("Include")?.Value == packageName)
            .ToList();

        if (packageReferenceElements.Count == 0)
        {
            return null;
        }

        var targetFrameworkNames = targetFrameworks.AsValueEnumerable().Select(tf => tf.TargetFramework).ToHashSet();

        foreach (var element in packageReferenceElements)
        {
            var conditions = element.GetConditionsIncludingParents();
            foreach (var condition in conditions)
            {
                if (
                    ConditionalTargetFrameworkResolver.TryExtractTargetFramework(condition, out var conditionalTfm)
                    && targetFrameworkNames.Contains(conditionalTfm)
                )
                {
                    return element.Attribute("Version");
                }
            }
        }

        foreach (var element in packageReferenceElements)
        {
            var conditions = element.GetConditionsIncludingParents();
            if (conditions.Count == 0)
            {
                return element.Attribute("Version");
            }
        }

        return packageReferenceElements[0].Attribute("Version");
    }
}
