using Aviationexam.DependencyUpdater.Common;
using System.Collections.Generic;

namespace Aviationexam.DependencyUpdater.Nuget.Models;

public record DependencyAnalysisResult(
    IReadOnlyDictionary<NugetDependency, IReadOnlyCollection<PossiblePackageVersion>> DependenciesToUpdate,
    IDictionary<Package, IDictionary<NugetTargetFramework, EDependencyFlag>> PackageFlags
);
