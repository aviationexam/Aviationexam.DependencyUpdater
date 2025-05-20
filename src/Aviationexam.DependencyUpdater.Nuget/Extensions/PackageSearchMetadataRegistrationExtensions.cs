using Aviationexam.DependencyUpdater.Common;
using NuGet.Protocol;
using System.Collections.Generic;

namespace Aviationexam.DependencyUpdater.Nuget.Extensions;

public static class PackageSearchMetadataRegistrationExtensions
{
    public static PackageVersion<PackageSearchMetadataRegistration> MapToPackageVersion(
        this PackageSearchMetadataRegistration packageSearchMetadataRegistration,
        EPackageSource packageSource
    ) => new(
        packageSearchMetadataRegistration.Version.Version,
        packageSearchMetadataRegistration.Version.IsPrerelease,
        [.. packageSearchMetadataRegistration.Version.ReleaseLabels],
        NugetReleaseLabelComparer.Instance,
        new Dictionary<EPackageSource, PackageSearchMetadataRegistration>
        {
            [packageSource] = packageSearchMetadataRegistration,
        }
    );
}
