using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.Extensions;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using System;

namespace Aviationexam.DependencyUpdater.Nuget;

public static class NugetMapper
{
    public static PackageVersion<PackageSearchMetadataRegistration> MapToPackageVersion(
        this IPackageSearchMetadata packageSearchMetadata,
        EPackageSource packageSource
    ) => packageSearchMetadata switch
    {
        PackageSearchMetadataRegistration packageSearchMetadataRegistration => PackageSearchMetadataRegistrationExtensions.MapToPackageVersion(packageSearchMetadataRegistration, packageSource),
        _ => throw new ArgumentOutOfRangeException(nameof(packageSearchMetadata), packageSearchMetadata, null),
    };
}
