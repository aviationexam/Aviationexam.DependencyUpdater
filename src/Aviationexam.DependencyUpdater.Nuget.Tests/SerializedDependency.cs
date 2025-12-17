namespace Aviationexam.DependencyUpdater.Nuget.Tests;

public sealed class SerializedDependency
{
    public required string Id { get; init; }
    public required string VersionRange { get; init; }
}
