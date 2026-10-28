using Aviationexam.DependencyUpdater.Nuget.Models;
using NuGet.Versioning;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Xml.Linq;
using ZLinq;

namespace Aviationexam.DependencyUpdater.Nuget.Parsers;

public static class NugetReleaseNotesParser
{
    public static NugetRegistrationIndex ParseRegistrationIndex(
        Stream stream
    )
    {
        using var document = JsonDocument.Parse(stream);

        var leaves = new List<NugetRegistrationLeaf>();
        var externalPageUrls = new List<string>();

        if (document.RootElement.TryGetProperty("items", out var pages) && pages.ValueKind is JsonValueKind.Array)
        {
            foreach (var page in pages.EnumerateArray())
            {
                if (page.TryGetProperty("items", out var items) && items.ValueKind is JsonValueKind.Array)
                {
                    leaves.AddRange(ParseLeaves(items));
                }
                else if (page.TryGetProperty("@id", out var pageUrl) && pageUrl.ValueKind is JsonValueKind.String)
                {
                    externalPageUrls.Add(pageUrl.GetString()!);
                }
            }
        }

        return new NugetRegistrationIndex(leaves, externalPageUrls);
    }

    public static IReadOnlyCollection<NugetRegistrationLeaf> ParseRegistrationPage(
        Stream stream
    )
    {
        using var document = JsonDocument.Parse(stream);

        return document.RootElement.TryGetProperty("items", out var items) && items.ValueKind is JsonValueKind.Array
            ? ParseLeaves(items)
            : [];
    }

    public static NugetNuspecMetadata ParseNuspecMetadata(
        Stream stream
    )
    {
        var document = XDocument.Load(stream);

        var metadata = document.Root
            ?.Elements().AsValueEnumerable().FirstOrDefault(x => x.Name.LocalName == "metadata");

        return new NugetNuspecMetadata(
            NullIfBlank(GetElement(metadata, "releaseNotes")?.Value),
            NullIfBlank(GetElement(metadata, "repository")?.Attribute("url")?.Value),
            NullIfBlank(GetElement(metadata, "projectUrl")?.Value)
        );
    }

    private static XElement? GetElement(
        XElement? parent,
        string localName
    ) => parent?.Elements().AsValueEnumerable().FirstOrDefault(x => x.Name.LocalName == localName);

    private static string? NullIfBlank(
        string? value
    ) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static List<NugetRegistrationLeaf> ParseLeaves(
        JsonElement items
    )
    {
        var leaves = new List<NugetRegistrationLeaf>();

        foreach (var item in items.EnumerateArray())
        {
            if (
                !item.TryGetProperty("catalogEntry", out var catalogEntry)
                || catalogEntry.ValueKind is not JsonValueKind.Object
                || !catalogEntry.TryGetProperty("version", out var versionElement)
                || versionElement.ValueKind is not JsonValueKind.String
                || !NuGetVersion.TryParse(versionElement.GetString(), out var version)
            )
            {
                continue;
            }

            var listed = !catalogEntry.TryGetProperty("listed", out var listedElement)
                         || listedElement.ValueKind is not JsonValueKind.False;

            leaves.Add(new NugetRegistrationLeaf(
                version,
                listed,
                GetString(catalogEntry, "releaseNotes"),
                GetString(catalogEntry, "projectUrl")
            ));
        }

        return leaves;
    }

    private static string? GetString(
        JsonElement element,
        string propertyName
    ) => element.TryGetProperty(propertyName, out var property) && property.ValueKind is JsonValueKind.String
        ? NullIfBlank(property.GetString())
        : null;
}
