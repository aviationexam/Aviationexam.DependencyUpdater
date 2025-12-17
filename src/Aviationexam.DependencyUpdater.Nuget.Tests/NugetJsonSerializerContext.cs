using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Aviationexam.DependencyUpdater.Nuget.Tests;

[JsonSerializable(typeof(IEnumerable<SerializedPackage>))]
[JsonSourceGenerationOptions(WriteIndented = true)]
public partial class NugetJsonSerializerContext : JsonSerializerContext;
