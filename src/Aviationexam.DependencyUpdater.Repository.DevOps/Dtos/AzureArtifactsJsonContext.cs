using System.Text.Json.Serialization;

namespace Aviationexam.DependencyUpdater.Repository.DevOps.Dtos;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = false
)]
[JsonSerializable(typeof(HierarchyQueryRequest))]
internal partial class AzureArtifactsJsonContext : JsonSerializerContext;
