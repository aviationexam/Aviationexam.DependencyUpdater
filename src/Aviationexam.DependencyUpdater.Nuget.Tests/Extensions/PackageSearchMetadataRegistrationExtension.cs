using NuGet.Packaging;
using NuGet.Protocol;
using NuGet.Versioning;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Aviationexam.DependencyUpdater.Nuget.Tests.Extensions;

public static class PackageSearchMetadataRegistrationExtension
{
    extension(PackageSearchMetadataRegistration packageSearchMetadataRegistration)
    {
        public PackageSearchMetadataRegistration SetPackageId(
            string packageId
        )
        {
            packageSearchMetadataRegistration.PackageIdField() = packageId;

            return packageSearchMetadataRegistration;
        }

        public PackageSearchMetadataRegistration SetVersion(
            NuGetVersion nuGetVersion
        )
        {
            packageSearchMetadataRegistration.VersionField() = nuGetVersion;

            return packageSearchMetadataRegistration;
        }

        public PackageSearchMetadataRegistration SetDependencySetsInternal(
            IEnumerable<PackageDependencyGroup> dependencySetsInternal
        )
        {
            packageSearchMetadataRegistration.DependencySetsInternalField() = dependencySetsInternal;

            return packageSearchMetadataRegistration;
        }
    }

    [SuppressMessage("ReSharper", "ConvertToExtensionBlock")]
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "<PackageId>k__BackingField")]
    private static extern ref string PackageIdField(this PackageSearchMetadata packageSearchMetadataRegistration);

    [SuppressMessage("ReSharper", "ConvertToExtensionBlock")]
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "<Version>k__BackingField")]
    private static extern ref NuGetVersion VersionField(this PackageSearchMetadata packageSearchMetadataRegistration);

    [SuppressMessage("ReSharper", "ConvertToExtensionBlock")]
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "<DependencySetsInternal>k__BackingField")]
    private static extern ref IEnumerable<PackageDependencyGroup> DependencySetsInternalField(this PackageSearchMetadata packageSearchMetadataRegistration);
}
