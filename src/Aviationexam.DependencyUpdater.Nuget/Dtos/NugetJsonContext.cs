using System.Text.Json.Serialization;

namespace Aviationexam.DependencyUpdater.Nuget.Dtos;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = false
)]
[JsonSerializable(typeof(VssNugetExternalFeedEndpoints))]
internal sealed partial class NugetJsonContext : JsonSerializerContext;
