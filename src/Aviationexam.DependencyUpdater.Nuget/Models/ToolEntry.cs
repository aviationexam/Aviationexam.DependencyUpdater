using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Aviationexam.DependencyUpdater.Nuget.Models;

public class ToolEntry
{
    [JsonPropertyName("version")]
    public required string Version { get; set; }

    [JsonPropertyName("commands")]
    public required IReadOnlyCollection<string> Commands { get; set; }
}
