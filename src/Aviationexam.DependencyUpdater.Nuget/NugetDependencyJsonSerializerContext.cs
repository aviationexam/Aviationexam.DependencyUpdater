using System.Text.Json.Serialization;

namespace Aviationexam.DependencyUpdater.Nuget;

[JsonSerializable(typeof(NugetDependency))]
internal sealed partial class NugetDependencyJsonSerializerContext : JsonSerializerContext;