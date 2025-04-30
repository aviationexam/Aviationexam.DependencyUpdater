using NuGet.Protocol.Core.Types;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;

namespace Aviationexam.DependencyUpdater.Nuget;

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
            includePrerelease: true,
            includeUnlisted: false,
            nugetCache,
            logger,
            cancellationToken
        );
    }
}
