using Corvus.Json;
using System.Diagnostics.CodeAnalysis;

namespace Aviationexam.DependencyUpdater.ConfigurationParser;

[JsonSchemaTypeGenerator("dependabot-2.0.json")]
[SuppressMessage("Design", "MA0077:A class that provides Equals(T) should implement IEquatable<T>")]
public readonly partial struct DependabotConfiguration;
