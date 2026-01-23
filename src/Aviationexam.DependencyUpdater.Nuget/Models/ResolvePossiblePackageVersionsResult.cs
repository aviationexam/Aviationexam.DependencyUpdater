using Aviationexam.DependencyUpdater.Common;
using System.Collections.Generic;

namespace Aviationexam.DependencyUpdater.Nuget.Models;

public sealed record ResolvePossiblePackageVersionsResult(
    IReadOnlyDictionary<UpdateCandidate, IReadOnlyCollection<PossiblePackageVersion>> DependencyToUpdate,
    CurrentPackageVersions CurrentPackageVersions
);
