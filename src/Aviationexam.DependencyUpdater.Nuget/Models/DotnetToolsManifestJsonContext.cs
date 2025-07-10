using System.Text.Json.Serialization;

namespace Aviationexam.DependencyUpdater.Nuget.Models;

[JsonSerializable(typeof(DotnetToolsManifest))]
public partial class DotnetToolsManifestJsonContext : JsonSerializerContext;
