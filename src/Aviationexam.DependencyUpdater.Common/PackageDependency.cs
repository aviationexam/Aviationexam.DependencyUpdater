namespace Aviationexam.DependencyUpdater.Common;

/// <summary>
/// Represents a dependency on another package.
/// Maps to NuGet.Packaging.Core.PackageDependency.
/// </summary>
public record PackageDependency(
    string Id,
    string? VersionRange
);
