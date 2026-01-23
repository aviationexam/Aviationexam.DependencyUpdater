using Aviationexam.DependencyUpdater.Common;
using System.Collections.Generic;

namespace Aviationexam.DependencyUpdater.Nuget.Models;

/// <summary>
/// Result of processing dependencies to update.
/// </summary>
public sealed record DependencyProcessingResult(
    IDictionary<Package, IDictionary<NugetTargetFramework, EDependencyFlag>> PackageFlags,
    Queue<(Package Package, NugetPackageCondition Condition, IReadOnlyCollection<NugetTargetFramework> NugetTargetFrameworks)> DependenciesToCheck
);
