using System.Collections.Generic;

namespace Aviationexam.DependencyUpdater.Nuget.Tests;

public sealed class SerializedPackage
{
    public required string PackageId { get; init; }
    public required string Version { get; init; }
    public required IReadOnlyCollection<SerializedDependencySet> DependencySets { get; init; }
}
