using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.Extensions;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Aviationexam.DependencyUpdater.Nuget;

public static class NugetMapper
{
    public static PackageVersion<PackageSearchMetadataRegistration>? MapToPackageVersion<T>(
        this IReadOnlyDictionary<EPackageSource, T> packageSearchMetadata
    ) where T : class, IPackageSearchMetadata => packageSearchMetadata
        .Select(x => x.Value.MapToPackageVersion())
        .FirstOrDefault()?
        .MapToPackageVersion(
            packageSearchMetadata
        );

    public static PackageVersion<PackageSearchMetadataRegistration> MapToPackageVersion<T>(
        this PackageVersion packageVersion,
        IReadOnlyDictionary<EPackageSource, T> packageSearchMetadata
    ) where T : class, IPackageSearchMetadata
    {
        if (packageSearchMetadata.All(x => x.Value is PackageSearchMetadataRegistration))
        {
            return PackageSearchMetadataRegistrationExtensions.MapToPackageVersion(
                packageVersion,
                packageSearchMetadata.ToDictionary(x => x.Key, x => (PackageSearchMetadataRegistration) (object) x.Value)
            );
        }

        throw new ArgumentOutOfRangeException(nameof(packageSearchMetadata), packageSearchMetadata, null);
    }

    public static PackageVersion MapToPackageVersion<T>(
        this T packageSearchMetadata
    ) where T : class, IPackageSearchMetadata
    {
        if (packageSearchMetadata is PackageSearchMetadataRegistration packageSearchMetadataRegistration)
        {
            return PackageSearchMetadataRegistrationExtensions.MapToPackageVersion(packageSearchMetadataRegistration);
        }

        throw new ArgumentOutOfRangeException(nameof(packageSearchMetadata), packageSearchMetadata, null);
    }
}
