namespace Aviationexam.DependencyUpdater.Common;

/// <summary>
/// Represents a dependency on another package.
/// Maps to NuGet.Packaging.Core.PackageDependency.
/// </summary>
public record PackageDependencyInfo(
    string Id,
    PackageVersion? MinVersion = null,
    bool IncludeMinVersion = true,
    PackageVersion? MaxVersion = null,
    bool IncludeMaxVersion = false,
    string? FloatRangeVersion = null,
    string? OriginalVersionString = null
);
