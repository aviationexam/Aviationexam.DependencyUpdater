using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.Models;
using NuGet.Protocol.Core.Types;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Aviationexam.DependencyUpdater.Nuget.Services;

public interface INugetVersionFetcher
{
    Task<IEnumerable<IPackageSearchMetadata>> FetchPackageVersionsAsync(
        SourceRepository repository,
        NugetDependency nugetDependency,
        SourceCacheContext nugetCache,
        CancellationToken cancellationToken
    );

    Task<IPackageSearchMetadata?> FetchPackageMetadataAsync(
        SourceRepository repository,
        Package package,
        SourceCacheContext cache,
        CancellationToken cancellationToken
    );
}
