using System.Text.Json.Serialization;

namespace Aviationexam.DependencyUpdater.Repository.DevOps.Dtos;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = false
)]
[JsonSerializable(typeof(AzSideCarRequest))]
[JsonSerializable(typeof(AzSideCarResponse))]
[JsonSerializable(typeof(HierarchyQueryRequest))]
[JsonSerializable(typeof(HierarchyQueryResponse))]
[JsonSerializable(typeof(ManualUpstreamIngestionRequest))]
internal sealed partial class AzureArtifactsJsonContext : JsonSerializerContext;
