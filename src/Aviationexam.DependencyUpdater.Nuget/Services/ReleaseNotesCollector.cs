using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.Extensions;
using Aviationexam.DependencyUpdater.Nuget.Models;
using Microsoft.Extensions.Logging;
using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ZLinq;

namespace Aviationexam.DependencyUpdater.Nuget.Services;

public sealed class ReleaseNotesCollector(
    INugetReleaseNotesFetcher releaseNotesFetcher,
    ILogger<ReleaseNotesCollector> logger
)
{
    public async Task<IReadOnlyCollection<PackageReleaseNote>> CollectReleaseNotesAsync(
        IReadOnlyCollection<NugetUpdateCandidate> updatedPackages,
        NugetUpdaterContext nugetUpdaterContext,
        IReadOnlyDictionary<NugetSource, NugetSourceRepository> sourceRepositories,
        CachingConfiguration cachingConfiguration,
        CancellationToken cancellationToken
    )
    {
        var updates = updatedPackages
            .AsValueEnumerable()
            .Select(x => (
                PackageName: x.NugetDependency.NugetPackage.GetPackageName(),
                FromVersion: x.NugetDependency.NugetPackage.GetVersion(),
                ToVersion: new PackageVersion(x.PossiblePackageVersion.PackageVersion)
            ))
            .Distinct()
            .OrderBy(x => x.PackageName)
            .ThenBy(x => x.ToVersion)
            .ToList();

        var results = new ConcurrentDictionary<int, IReadOnlyCollection<PackageReleaseNote>>();

        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount / 2),
            CancellationToken = cancellationToken,
        };

        try
        {
            await Parallel.ForEachAsync(updates.AsValueEnumerable().Index().ToList(), parallelOptions, async (update, token) =>
            {
                var (index, (packageName, fromVersion, toVersion)) = update;

                results[index] = await CollectPackageReleaseNotesAsync(
                    packageName,
                    fromVersion,
                    toVersion,
                    nugetUpdaterContext,
                    sourceRepositories,
                    cachingConfiguration,
                    token
                );
            });
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning(ex, "Failed to collect release notes, the pull request description will not contain them");
            }
        }

        return results
            .AsValueEnumerable()
            .OrderBy(x => x.Key)
            .SelectMany(x => x.Value)
            .ToList();
    }

    private async Task<IReadOnlyCollection<PackageReleaseNote>> CollectPackageReleaseNotesAsync(
        string packageName,
        PackageVersion? fromVersion,
        PackageVersion toVersion,
        NugetUpdaterContext nugetUpdaterContext,
        IReadOnlyDictionary<NugetSource, NugetSourceRepository> sourceRepositories,
        CachingConfiguration cachingConfiguration,
        CancellationToken cancellationToken
    )
    {
        try
        {
            using var nugetCache = new SourceCacheContext();
            nugetCache.MaxAge = cachingConfiguration.MaxCacheAge;

            foreach (var nugetSource in nugetUpdaterContext.GetSourcesForPackage(packageName, logger))
            {
                if (!sourceRepositories.TryGetValue(nugetSource, out var sourceRepository))
                {
                    continue;
                }

                foreach (var repository in new[] { sourceRepository.SourceRepository, sourceRepository.FallbackSourceRepository })
                {
                    if (repository is null)
                    {
                        continue;
                    }

                    var releaseNotes = await releaseNotesFetcher.FetchReleaseNotesAsync(
                        repository,
                        packageName,
                        fromVersion,
                        toVersion,
                        nugetCache,
                        cancellationToken
                    );

                    if (releaseNotes.Count > 0)
                    {
                        return releaseNotes;
                    }
                }
            }

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("No release notes found for {PackageName} {From} -> {To}", packageName, fromVersion?.GetSerializedVersion(), toVersion.GetSerializedVersion());
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning(ex, "Failed to collect release notes for {PackageName} {From} -> {To}", packageName, fromVersion?.GetSerializedVersion(), toVersion.GetSerializedVersion());
            }
        }

        return [];
    }
}
