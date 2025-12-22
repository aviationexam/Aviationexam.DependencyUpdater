using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.Services;
using NuGet.Packaging;
using NuGet.Protocol;
using System.Collections.Generic;
using System.Linq;
using ZLinq;

namespace Aviationexam.DependencyUpdater.Nuget.Extensions;

public static class PackageSearchMetadataRegistrationExtensions
{
    public static PackageVersion<PackageSearchMetadataRegistration> MapToPackageVersion(
        this PackageVersion packageVersion,
        IReadOnlyDictionary<EPackageSource, PackageSearchMetadataRegistration> packageSearchMetadata
    ) => new(
        packageVersion,
        packageSearchMetadata
    )
    {
        DependencySets = MapDependencySets(packageSearchMetadata)
    };

    public static PackageVersion MapToPackageVersion(
        this PackageSearchMetadataRegistration packageSearchMetadataRegistration
    ) => new(
        packageSearchMetadataRegistration.Version.Version,
        packageSearchMetadataRegistration.Version.IsPrerelease,
        [.. packageSearchMetadataRegistration.Version.ReleaseLabels],
        NugetReleaseLabelComparer.Instance
    );

    private static IReadOnlyCollection<DependencySet> MapDependencySets(
        IReadOnlyDictionary<EPackageSource, PackageSearchMetadataRegistration> packageSearchMetadata
    )
    {
        // Use the first available source's dependency sets
        var firstMetadata = packageSearchMetadata.Values.AsValueEnumerable().FirstOrDefault();
        if (firstMetadata is null)
        {
            return [];
        }

        return MapDependencySets(firstMetadata.DependencySets);
    }

    private static IReadOnlyCollection<DependencySet> MapDependencySets(
        IEnumerable<PackageDependencyGroup> dependencySets
    ) => dependencySets.AsValueEnumerable().Select(group => new DependencySet(
        group.TargetFramework.GetShortFolderName(),
        group.Packages.AsValueEnumerable().Select(package => new PackageDependencyInfo(
            package.Id,
            package.VersionRange?.ToString()
        )).ToList()
    )).ToList();
}
