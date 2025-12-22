using System.Collections.Generic;

namespace Aviationexam.DependencyUpdater.Common;

/// <summary>
/// Represents a set of package dependencies for a specific target framework.
/// Maps to NuGet.Packaging.PackageDependencyGroup.
/// </summary>
public record DependencySet(
    string TargetFramework,
    IReadOnlyCollection<PackageDependencyInfo> Packages
);
