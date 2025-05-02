using Aviationexam.DependencyUpdater.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using System;

namespace Aviationexam.DependencyUpdater.Nuget;

public static class NugetMapper
{
    public static PackageVersion<PackageSearchMetadataRegistration> MapToPackageVersion(
        this IPackageSearchMetadata packageSearchMetadata
    ) => packageSearchMetadata switch
    {
        PackageSearchMetadataRegistration packageSearchMetadataRegistration => packageSearchMetadataRegistration.MapToPackageVersion(),
        _ => throw new ArgumentOutOfRangeException(nameof(packageSearchMetadata), packageSearchMetadata, null),
    };
}
