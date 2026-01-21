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

    /// <param name="dependencies">Collection of NuGet dependencies with their available versions</param>
    extension(IReadOnlyCollection<KeyValuePair<NugetDependency, IReadOnlyCollection<PackageVersionWithDependencySets>>> dependencies)
    {
        /// <summary>
        /// Converts a collection of NuGet dependencies with version sets to a dictionary of possible package versions.
        /// Extracts the default package source dependency sets for each package version.
        /// </summary>
        /// <returns>Dictionary mapping NuGet dependencies to collections of possible package versions with their compatible dependency sets</returns>
        public IReadOnlyDictionary<UpdateCandidate, IReadOnlyCollection<PossiblePackageVersion>> ToPossiblePackageVersions() => dependencies
            .AsValueEnumerable()
            .ToDictionary(
                kvp => new UpdateCandidate(
                    kvp.Key, new PackageVersionWithDependencySets(
                        kvp.Key.NugetPackage.GetVersion()!
                    )
                    {
                        DependencySets = new Dictionary<EPackageSource, IReadOnlyCollection<DependencySet>>()
                    }
                ),
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

        /// <summary>
        /// Converts a collection of NuGet dependencies to a dictionary of current package versions per target framework.
        /// Extracts the MinVersion from each NugetPackageReference and maps it to the corresponding target frameworks.
        /// </summary>
        /// <returns>Dictionary mapping package names to their versions per target framework</returns>
        public IReadOnlyDictionary<string, IDictionary<string, PackageVersion>> ToCurrentVersionsPerTargetFramework()
        {
            var currentVersions = new Dictionary<string, IDictionary<string, PackageVersion>>();

            foreach (var (nugetDependency, _) in dependencies)
            {
                if (
                    nugetDependency.NugetPackage is NugetPackageReference { Name: var packageReferenceName } packageRef
                    && packageRef.MapMinVersionToPackageVersion() is { } currentVersion
                )
                {
                    if (!currentVersions.TryGetValue(packageReferenceName, out var frameworkVersions))
                    {
                        frameworkVersions = new Dictionary<string, PackageVersion>();
                        currentVersions[packageReferenceName] = frameworkVersions;
                    }

                    foreach (var targetFramework in nugetDependency.TargetFrameworks)
                    {
                        frameworkVersions[targetFramework.TargetFramework] = currentVersion;
                    }
                }
                else if (
                    nugetDependency.NugetPackage is NugetPackageVersion { Name: var packageName } packageVersion
                    && packageVersion.Version.MapToPackageVersion() is { } currentPackageVersion
                )
                {
                    if (!currentVersions.TryGetValue(packageName, out var frameworkVersions))
                    {
                        frameworkVersions = new Dictionary<string, PackageVersion>();
                        currentVersions[packageName] = frameworkVersions;
                    }

                    foreach (var targetFramework in nugetDependency.TargetFrameworks)
                    {
                        frameworkVersions[targetFramework.TargetFramework] = currentPackageVersion;
                    }
                }
            }

            return currentVersions;
        }
    }
}
