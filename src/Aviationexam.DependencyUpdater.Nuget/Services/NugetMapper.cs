using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.Extensions;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Generic;
using ZLinq;

namespace Aviationexam.DependencyUpdater.Nuget.Services;

public static class NugetMapper
{
    public static PackageVersionWithDependencySets? MapToPackageVersionWithDependencySets<T>(
        this IReadOnlyDictionary<EPackageSource, T> packageSearchMetadata
    ) where T : class, IPackageSearchMetadata
    {
        if (!packageSearchMetadata.AsValueEnumerable().All(x => x.Value is PackageSearchMetadataRegistration))
        {
            throw new ArgumentOutOfRangeException(nameof(packageSearchMetadata), packageSearchMetadata, "All metadata must be PackageSearchMetadataRegistration");
        }

        var packageVersion = packageSearchMetadata
            .AsValueEnumerable()
            .Select(x => x.Value.MapToPackageVersion())
            .FirstOrDefault();

        if (packageVersion is null)
        {
            return null;
        }

        var registrationMetadata = packageSearchMetadata.AsValueEnumerable()
            .ToDictionary(
                x => x.Key,
                x => (PackageSearchMetadataRegistration) (object) x.Value
            );

        return packageVersion.MapToPackageVersionWithDependencySets(registrationMetadata);
    }

    public static PackageVersionWithDependencySets MapToPackageVersionWithDependencySets<T>(
        this PackageVersion packageVersion,
        IReadOnlyDictionary<EPackageSource, T> packageSearchMetadata
    ) where T : class, IPackageSearchMetadata
    {
        if (packageSearchMetadata.AsValueEnumerable().All(x => x.Value is PackageSearchMetadataRegistration))
        {
            return PackageSearchMetadataRegistrationExtensions.MapToPackageVersionWithDependencySets(
                packageVersion,
                packageSearchMetadata.AsValueEnumerable()
                    .ToDictionary(x => x.Key, x => (PackageSearchMetadataRegistration) (object) x.Value)
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
