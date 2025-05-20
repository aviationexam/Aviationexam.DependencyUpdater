using Aviationexam.DependencyUpdater.Common;
using NuGet.Protocol;
using System.Collections.Generic;

namespace Aviationexam.DependencyUpdater.Nuget.Extensions;

public static class PackageSearchMetadataRegistrationExtensions
{
    public static PackageVersion<PackageSearchMetadataRegistration> MapToPackageVersion(
        this PackageVersion packageVersion,
        IReadOnlyDictionary<EPackageSource, PackageSearchMetadataRegistration> packageSearchMetadata
    ) => new(
        packageVersion,
        packageSearchMetadata
    );

    public static PackageVersion MapToPackageVersion(
        this PackageSearchMetadataRegistration packageSearchMetadataRegistration
    ) => new(
        packageSearchMetadataRegistration.Version.Version,
        packageSearchMetadataRegistration.Version.IsPrerelease,
        [.. packageSearchMetadataRegistration.Version.ReleaseLabels],
        NugetReleaseLabelComparer.Instance
    );
}
