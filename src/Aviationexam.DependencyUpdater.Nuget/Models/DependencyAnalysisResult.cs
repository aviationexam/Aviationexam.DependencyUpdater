using Aviationexam.DependencyUpdater.Common;
using System.Collections.Generic;

namespace Aviationexam.DependencyUpdater.Nuget.Models;

public record DependencyAnalysisResult(
    IReadOnlyDictionary<UpdateCandidate, IReadOnlyCollection<PossiblePackageVersion>> DependenciesToUpdate,
    CurrentPackageVersions CurrentPackageVersions,
    IDictionary<Package, IDictionary<NugetTargetFramework, EDependencyFlag>> PackageFlags
);
