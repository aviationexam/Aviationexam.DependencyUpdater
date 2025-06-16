using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Interfaces;
using Aviationexam.DependencyUpdater.Nuget.Extensions;
using Aviationexam.DependencyUpdater.Nuget.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Aviationexam.DependencyUpdater.Nuget.Writers;

public sealed class NugetCsprojVersionWriter(
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

        await using var fileStream = fileSystem.FileOpen(fullPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);

        var doc = await XDocument.LoadAsync(fileStream, LoadOptions.PreserveWhitespace, cancellationToken);

        var versionAttribute = doc
            .Descendants()
            .Where(e => e.Name.LocalName == "PackageReference")
            .Where(e => e.Attribute("Include")?.Value == packageName)
            .Select(x => x.Attribute("Version"))
            .SingleOrDefault();

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
            DoNotEscapeUriAttributes = false,
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
}
