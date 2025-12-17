using System.Collections.Generic;

namespace Aviationexam.DependencyUpdater.Nuget.Tests;

public sealed class SerializedDependencySet
{
    public required string Framework { get; init; }
    public required string Version { get; init; }
    public required string Platform { get; init; }
    public required string PlatformVersion { get; init; }
    public required IReadOnlyCollection<SerializedDependency> Packages { get; init; }
}
