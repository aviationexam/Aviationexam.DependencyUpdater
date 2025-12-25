using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Aviationexam.DependencyUpdater.Nuget.Services;

public interface IDependencyVersionsFetcher
{
    public const string Real = "Real";

    Task<IReadOnlyCollection<PackageVersionWithDependencySets>> FetchDependencyVersionsAsync(
        NugetDependency dependency,
        IReadOnlyCollection<NugetSource> sources,
        IReadOnlyDictionary<NugetSource, NugetSourceRepository> sourceRepositories,
        CachingConfiguration cachingConfiguration,
        CancellationToken cancellationToken
    );

    Task<PackageVersionWithDependencySets?> FetchPackageMetadataAsync(
        Package package,
        IReadOnlyCollection<NugetSource> sources,
        IReadOnlyDictionary<NugetSource, NugetSourceRepository> sourceRepositories,
        CachingConfiguration cachingConfiguration,
        CancellationToken cancellationToken
    );
}
