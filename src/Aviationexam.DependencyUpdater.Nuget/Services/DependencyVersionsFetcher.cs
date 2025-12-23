using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.Models;
using NuGet.Protocol.Core.Types;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ZLinq;

namespace Aviationexam.DependencyUpdater.Nuget.Services;

public sealed class DependencyVersionsFetcher(
    INugetVersionFetcher nugetVersionFetcher
) : IDependencyVersionsFetcher
{
    public async Task<IReadOnlyCollection<PackageVersionWithDependencySets>> FetchDependencyVersionsAsync(
        NugetDependency dependency,
        IReadOnlyCollection<NugetSource> sources,
        IReadOnlyDictionary<NugetSource, NugetSourceRepository> sourceRepositories,
        CachingConfiguration cachingConfiguration,
        CancellationToken cancellationToken
    )
    {
        var tasks = sources.Select(async Task<IReadOnlyCollection<PackageVersionWithDependencySets>> (nugetSource) =>
        {
            if (sourceRepositories.TryGetValue(nugetSource, out var sourceRepository))
            {
                using var nugetCache = new SourceCacheContext();

                nugetCache.MaxAge = cachingConfiguration.MaxCacheAge;

                var rawPackageVersions = await nugetVersionFetcher.FetchPackageVersionsAsync(
                    sourceRepository.SourceRepository,
                    dependency,
                    nugetCache,
                    cancellationToken
                );
                var packageVersions = rawPackageVersions.Select(x => (Metadata: x, PackageVersion: x.MapToPackageVersion(), PackageSource: EPackageSource.Default));

                if (sourceRepository.FallbackSourceRepository is { } fallbackSourceRepository)
                {
                    var fallbackPackageVersions = await nugetVersionFetcher.FetchPackageVersionsAsync(
                        fallbackSourceRepository,
                        dependency,
                        nugetCache,
                        cancellationToken
                    );

                    packageVersions = packageVersions.Concat(fallbackPackageVersions.Select(x => (Metadata: x, PackageVersion: x.MapToPackageVersion(), PackageSource: EPackageSource.Fallback)));
                }

                return packageVersions
                    .GroupBy(x => x.PackageVersion)
                    .Select(x => KeyValuePair.Create(
                        x.Key,
                        x.AsValueEnumerable().ToDictionary(
                            d => d.PackageSource,
                            d => d.Metadata
                        )
                    ))
                    .Select(x => x.Key.MapToPackageVersionWithDependencySets(x.Value))
                    .ToList();
            }

            return [];
        });

        var results = await Task.WhenAll(tasks);

        return results.AsValueEnumerable().SelectMany(x => x).ToList();
    }
}
