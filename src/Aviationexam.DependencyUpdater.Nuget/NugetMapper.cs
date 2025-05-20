using Aviationexam.DependencyUpdater.Common;
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
        PackageSearchMetadataRegistration packageSearchMetadataRegistration => packageSearchMetadataRegistration.MapToPackageVersion(packageSource),
        _ => throw new ArgumentOutOfRangeException(nameof(packageSearchMetadata), packageSearchMetadata, null),
    };
}
