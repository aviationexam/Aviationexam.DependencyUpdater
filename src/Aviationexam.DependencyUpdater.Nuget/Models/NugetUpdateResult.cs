using Aviationexam.DependencyUpdater.Common;
using System.Collections.Generic;

namespace Aviationexam.DependencyUpdater.Nuget.Models;

public sealed record NugetUpdateResult(
    NugetUpdateCandidate UpdateCandidate,
    IReadOnlyDictionary<string, PackageVersion> FromVersionsPerFramework
);
