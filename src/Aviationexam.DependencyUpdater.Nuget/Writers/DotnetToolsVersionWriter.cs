using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Interfaces;
using Aviationexam.DependencyUpdater.Nuget.Extensions;
using Aviationexam.DependencyUpdater.Nuget.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Aviationexam.DependencyUpdater.Nuget.Writers;

public sealed class DotnetToolsVersionWriter(
    IFileSystem fileSystem,
    ILogger<DotnetToolsVersionWriter> logger
)
{
    public async Task<ESetVersion> TrySetVersionAsync(
        NugetUpdateCandidate nugetUpdateCandidate,
        string fullPath,
        IDictionary<string, IDictionary<string, PackageVersion>> groupPackageVersions,
        CancellationToken cancellationToken
    )
    {
        var packageName = nugetUpdateCandidate.NugetDependency.NugetPackage.GetPackageName();

        await using var fileStream = fileSystem.FileOpen(fullPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

        JsonNode? jsonNode;
        try
        {
            jsonNode = await JsonNode.ParseAsync(fileStream, new JsonNodeOptions
            {
                PropertyNameCaseInsensitive = false,
            }, new JsonDocumentOptions
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Disallow,
                MaxDepth = 10,
            }, cancellationToken);
        }
        catch (JsonException e)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                logger.LogError(e, "Unable to parse in dotnet-tools.json: {path}", fullPath);
            }

            return ESetVersion.VersionNotSet;
        }

        if (jsonNode is null)
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning("Unable to parse in dotnet-tools.json: {path}", fullPath);
            }

            return ESetVersion.VersionNotSet;
        }

        if (
            !jsonNode.AsObject().TryGetPropertyValue("tools", out var tools)
            || tools is null
        )
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning("No tools found in dotnet-tools.json: {path}", fullPath);
            }

            return ESetVersion.VersionNotSet;
        }

        if (
            tools.AsObject().TryGetPropertyValue(packageName, out var toolEntryNode)
            && toolEntryNode?.AsObject() is { } toolEntryObject
        )
        {
            toolEntryObject["version"] = nugetUpdateCandidate.PossiblePackageVersion.PackageVersion.GetSerializedVersion();

            if (groupPackageVersions.TryGetValue(packageName, out var frameworkVersions))
            {
                // Update version for each target framework this dependency applies to
                foreach (var targetFramework in nugetUpdateCandidate.NugetDependency.TargetFrameworks)
                {
                    frameworkVersions[targetFramework.TargetFramework] = nugetUpdateCandidate.PossiblePackageVersion.PackageVersion;
                }
            }

            fileStream.SetLength(0);
            fileStream.Seek(0, SeekOrigin.Begin);

            await using var writer = new Utf8JsonWriter(fileStream, new JsonWriterOptions
            {
                Encoder = null,
                Indented = true,
                IndentCharacter = ' ',
                IndentSize = 2,
                NewLine = "\n",
                MaxDepth = 10,
                SkipValidation = false,
            });
            jsonNode.WriteTo(writer, new JsonSerializerOptions
            {
                AllowOutOfOrderMetadataProperties = false,
                AllowTrailingCommas = false,
                DefaultIgnoreCondition = JsonIgnoreCondition.Never,
                DictionaryKeyPolicy = null,
                Encoder = null,
                IgnoreReadOnlyFields = false,
                IgnoreReadOnlyProperties = false,
                IncludeFields = false,
                MaxDepth = 10,
                NewLine = "\n",
                NumberHandling = JsonNumberHandling.Strict,
                PreferredObjectCreationHandling = JsonObjectCreationHandling.Replace,
                PropertyNameCaseInsensitive = false,
                PropertyNamingPolicy = null,
                ReadCommentHandling = JsonCommentHandling.Disallow,
                ReferenceHandler = null,
                RespectNullableAnnotations = true,
                RespectRequiredConstructorParameters = true,
                TypeInfoResolver = null,
                UnknownTypeHandling = JsonUnknownTypeHandling.JsonElement,
                UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
                WriteIndented = true,
                IndentCharacter = ' ',
                IndentSize = 2,
            });
            await writer.FlushAsync(cancellationToken);

            return ESetVersion.VersionSet;
        }

        return ESetVersion.VersionNotSet;
    }
}
