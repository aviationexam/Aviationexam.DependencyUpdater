using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.Models;
using NuGet.Versioning;
using System.Collections.Generic;
using ZLinq;

namespace Aviationexam.DependencyUpdater.Nuget.Extensions;

public static class PackageVersionExtensions
{
    public static NuGetVersion MapToNuGetVersion(
        this PackageVersion packageVersion
    ) => new(packageVersion.Version, string.Join('.', packageVersion.ReleaseLabels));

    /// <summary>
    /// Converts a collection of NuGet dependencies with version sets to a dictionary of possible package versions.
    /// Extracts the default package source dependency sets for each package version.
    /// </summary>
    /// <param name="dependencies">Collection of NuGet dependencies with their available versions</param>
    /// <returns>Dictionary mapping NuGet dependencies to collections of possible package versions with their compatible dependency sets</returns>
    public static IReadOnlyDictionary<NugetDependency, IReadOnlyCollection<PossiblePackageVersion>> ToPossiblePackageVersions(
        this IReadOnlyCollection<KeyValuePair<NugetDependency, IReadOnlyCollection<PackageVersionWithDependencySets>>> dependencies
    ) => dependencies.AsValueEnumerable()
        .ToDictionary(
            kvp => kvp.Key,
            IReadOnlyCollection<PossiblePackageVersion> (kvp) =>
            [
                .. kvp.Value.AsValueEnumerable()
                    .Select(pkgVersion => new PossiblePackageVersion(
                        pkgVersion,
                        pkgVersion.DependencySets.TryGetValue(EPackageSource.Default, out var depSets)
                            ? depSets
                            : []
                    )),
            ]
        );
}
