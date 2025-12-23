using Aviationexam.DependencyUpdater.Common;
using System.Collections.Generic;

namespace Aviationexam.DependencyUpdater.Nuget.Models;

public record PossiblePackageVersion(
    PackageVersionWithDependencySets PackageVersion,
    IReadOnlyCollection<DependencySet> CompatibleDependencySets
);
