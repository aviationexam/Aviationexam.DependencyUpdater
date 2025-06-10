using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.Models;
using System.Collections.Generic;

namespace Aviationexam.DependencyUpdater.Nuget.Services;

public record DependencyAnalysisResult(
    IReadOnlyDictionary<NugetDependency, IReadOnlyCollection<PossiblePackageVersion>> DependenciesToUpdate,
    IDictionary<Package, EDependencyFlag> PackageFlags
);
