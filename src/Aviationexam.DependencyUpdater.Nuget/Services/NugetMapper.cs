using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.Extensions;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Generic;
using System.Linq;
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
            .Select(x => MapPackageSearchMetadataToPackageVersion(x.Value))
            .FirstOrDefault();

        if (packageVersion is null)
        {
            return null;
        }

        var registrationMetadata = packageSearchMetadata.AsValueEnumerable()
            .ToDictionary(x => x.Key, x => (PackageSearchMetadataRegistration)(object)x.Value);

        return packageVersion.MapToPackageVersionWithDependencySets(registrationMetadata);
    }

    private static PackageVersion MapPackageSearchMetadataToPackageVersion<T>(
        T packageSearchMetadata
    ) where T : class, IPackageSearchMetadata
    {
        if (packageSearchMetadata is not PackageSearchMetadataRegistration registration)
        {
            throw new ArgumentOutOfRangeException(nameof(packageSearchMetadata), packageSearchMetadata, "Metadata must be PackageSearchMetadataRegistration");
        }

        return new PackageVersion(
            registration.Version.Version,
            registration.Version.IsPrerelease,
            [.. registration.Version.ReleaseLabels],
            NugetReleaseLabelComparer.Instance
        );
    }
}
