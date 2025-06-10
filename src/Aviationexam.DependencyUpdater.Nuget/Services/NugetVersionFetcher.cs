using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.Extensions;
using Aviationexam.DependencyUpdater.Nuget.Models;
using NuGet.Common;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Aviationexam.DependencyUpdater.Nuget.Services;

public sealed class NugetVersionFetcher(
    ILogger logger
)
{
    public async Task<IEnumerable<IPackageSearchMetadata>> FetchPackageVersionsAsync(
        SourceRepository repository,
        NugetDependency nugetDependency,
        SourceCacheContext nugetCache,
        CancellationToken cancellationToken
    )
    {
        var resource = await repository.GetResourceAsync<PackageMetadataResource>(cancellationToken);

        return await resource.GetMetadataAsync(
            nugetDependency.NugetPackage.GetPackageName(),
            includePrerelease: nugetDependency.NugetPackage.GetVersion()?.IsPrerelease ?? true,
            includeUnlisted: false,
            nugetCache,
            logger,
            cancellationToken
        );
    }

    public async Task<IPackageSearchMetadata?> FetchPackageMetadataAsync(
        SourceRepository repository,
        Package package,
        SourceCacheContext cache,
        CancellationToken cancellationToken
    )
    {
        var resource = await repository.GetResourceAsync<PackageMetadataResource>(cancellationToken);

        var identity = new PackageIdentity(package.Name, package.Version.MapToNuGetVersion());

        return await resource.GetMetadataAsync(identity, cache, logger, cancellationToken);
    }
}
