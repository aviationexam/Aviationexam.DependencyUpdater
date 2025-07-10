namespace Aviationexam.DependencyUpdater.Nuget.Models;

using System.Collections.Generic;
using System.Text.Json.Serialization;

public class DotnetToolsManifest
{
    [JsonPropertyName("version")]
    public required int Version { get; set; }

    [JsonPropertyName("isRoot")]
    public required bool IsRoot { get; set; }

    [JsonPropertyName("tools")]
    public required IReadOnlyDictionary<string, ToolEntry> Tools { get; set; }
}
