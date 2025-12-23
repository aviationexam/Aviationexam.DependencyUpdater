using Aviationexam.DependencyUpdater.Common;
using NuGet.Packaging;
using NuGet.Protocol;
using System.Collections.Generic;
using ZLinq;

namespace Aviationexam.DependencyUpdater.Nuget.Extensions;

public static class PackageSearchMetadataRegistrationExtensions
{
    public static PackageVersionWithDependencySets MapToPackageVersionWithDependencySets(
        this PackageVersion packageVersion,
        IReadOnlyDictionary<EPackageSource, PackageSearchMetadataRegistration> packageSearchMetadata
    ) => new(
        packageVersion
    )
    {
        DependencySets = MapDependencySets(packageSearchMetadata),
    };

    private static IReadOnlyDictionary<EPackageSource, IReadOnlyCollection<DependencySet>> MapDependencySets(
        IReadOnlyDictionary<EPackageSource, PackageSearchMetadataRegistration> packageSearchMetadata
    ) => packageSearchMetadata.AsValueEnumerable().ToDictionary(
        kvp => kvp.Key,
        kvp => MapDependencySetsForSource(kvp.Value.DependencySets)
    );

    private static IReadOnlyCollection<DependencySet> MapDependencySetsForSource(
        IEnumerable<PackageDependencyGroup> dependencySets
    ) => dependencySets.AsValueEnumerable().Select(group => new DependencySet(
        group.TargetFramework.GetShortFolderName(),
        group.Packages.AsValueEnumerable().Select(package => new PackageDependencyInfo(
            package.Id,
            MinVersion: package.VersionRange.MinVersion?.MapToPackageVersion(),
            IncludeMinVersion: package.VersionRange.IsMinInclusive,
            MaxVersion: package.VersionRange.MaxVersion?.MapToPackageVersion(),
            IncludeMaxVersion: package.VersionRange.IsMaxInclusive,
            FloatRangeVersion: package.VersionRange.Float?.ToString(),
            OriginalVersionString: package.VersionRange.OriginalString
        )).ToList()
    )).ToList();
}
