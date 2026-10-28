using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.Extensions;
using Aviationexam.DependencyUpdater.Nuget.Models;
using Aviationexam.DependencyUpdater.Nuget.Parsers;
using Microsoft.Extensions.Logging;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZLinq;
using ILogger = NuGet.Common.ILogger;

namespace Aviationexam.DependencyUpdater.Nuget.Services;

public sealed class NugetReleaseNotesFetcher(
    ILogger nugetLogger,
    ILogger<NugetReleaseNotesFetcher> logger
) : INugetReleaseNotesFetcher
{
    public const int MaxVersionsPerPackage = 50;

    public async Task<IReadOnlyCollection<PackageReleaseNote>> FetchReleaseNotesAsync(
        SourceRepository repository,
        string packageName,
        PackageVersion? fromVersion,
        PackageVersion toVersion,
        SourceCacheContext nugetCache,
        CancellationToken cancellationToken
    )
    {
        if (
            await repository.GetResourceAsync<ServiceIndexResourceV3>(cancellationToken) is not { } serviceIndex
            || await repository.GetResourceAsync<HttpSourceResource>(cancellationToken) is not { HttpSource: { } httpSource }
            || serviceIndex.GetServiceEntryUri(ServiceTypes.RegistrationsBaseUrl) is not { } registrationsBaseUrl
        )
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("Source {Source} does not expose V3 registration metadata, skipping release notes for {PackageName}", repository.PackageSource.Source, packageName);
            }

            return [];
        }

        var packageBaseAddress = serviceIndex.GetServiceEntryUri(ServiceTypes.PackageBaseAddress);
        var lowerId = packageName.ToLowerInvariant();
        var cacheContext = HttpSourceCacheContext.Create(nugetCache, isFirstAttempt: true);

        var leaves = await FetchRegistrationLeavesAsync(httpSource, registrationsBaseUrl, lowerId, cacheContext, cancellationToken);

        var versions = SelectVersions(leaves, fromVersion, toVersion);

        if (versions.Count > MaxVersionsPerPackage)
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning("Package {PackageName} has {Count} versions between {From} and {To}, release notes are limited to the newest {Max}", packageName, versions.Count, fromVersion?.GetSerializedVersion(), toVersion.GetSerializedVersion(), MaxVersionsPerPackage);
            }

            versions = versions.AsValueEnumerable().TakeLast(MaxVersionsPerPackage).ToList();
        }

        var releaseNotes = new List<PackageReleaseNote>();

        foreach (var leaf in versions)
        {
            var notes = leaf.ReleaseNotes;

            if (notes is null && packageBaseAddress is not null)
            {
                notes = await FetchNuspecReleaseNotesAsync(httpSource, packageBaseAddress, lowerId, leaf.Version, cacheContext, cancellationToken);
            }

            if (notes is not null)
            {
                releaseNotes.Add(new PackageReleaseNote(packageName, leaf.Version.ToNormalizedString(), notes));
            }
        }

        return releaseNotes;
    }

    public static IReadOnlyList<NugetRegistrationLeaf> SelectVersions(
        IEnumerable<NugetRegistrationLeaf> leaves,
        PackageVersion? fromVersion,
        PackageVersion toVersion
    )
    {
        var from = fromVersion?.MapToNuGetVersion();
        var to = toVersion.MapToNuGetVersion();

        // Prerelease versions passed on the way to a stable release are not part of the stable release line.
        return leaves
            .AsValueEnumerable()
            .Where(x => x.Listed)
            .Where(x => x.Version <= to && (from is null ? x.Version == to : x.Version > from))
            .Where(x => to.IsPrerelease || !x.Version.IsPrerelease)
            .DistinctBy(x => x.Version)
            .OrderBy(x => x.Version)
            .ToList();
    }

    private async Task<IReadOnlyCollection<NugetRegistrationLeaf>> FetchRegistrationLeavesAsync(
        HttpSource httpSource,
        Uri registrationsBaseUrl,
        string lowerId,
        HttpSourceCacheContext cacheContext,
        CancellationToken cancellationToken
    )
    {
        var indexUrl = $"{registrationsBaseUrl.AbsoluteUri.TrimEnd('/')}/{lowerId}/index.json";

        var index = await GetAsync(
            httpSource,
            indexUrl,
            $"release_notes_registration_{lowerId}_index",
            cacheContext,
            NugetReleaseNotesParser.ParseRegistrationIndex,
            cancellationToken
        );

        if (index is null)
        {
            return [];
        }

        var leaves = new List<NugetRegistrationLeaf>(index.Leaves);

        foreach (var pageUrl in index.ExternalPageUrls)
        {
            var pageLeaves = await GetAsync(
                httpSource,
                pageUrl,
                $"release_notes_registration_{lowerId}_page_{Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(pageUrl)))}",
                cacheContext,
                NugetReleaseNotesParser.ParseRegistrationPage,
                cancellationToken
            );

            if (pageLeaves is not null)
            {
                leaves.AddRange(pageLeaves);
            }
        }

        return leaves;
    }

    private async Task<string?> FetchNuspecReleaseNotesAsync(
        HttpSource httpSource,
        Uri packageBaseAddress,
        string lowerId,
        NuGetVersion version,
        HttpSourceCacheContext cacheContext,
        CancellationToken cancellationToken
    )
    {
        var lowerVersion = version.ToNormalizedString().ToLowerInvariant();
        var nuspecUrl = $"{packageBaseAddress.AbsoluteUri.TrimEnd('/')}/{lowerId}/{lowerVersion}/{lowerId}.nuspec";

        return await GetAsync(
            httpSource,
            nuspecUrl,
            $"release_notes_nuspec_{lowerId}_{lowerVersion}",
            cacheContext,
            NugetReleaseNotesParser.ParseNuspecReleaseNotes,
            cancellationToken
        );
    }

    private Task<T?> GetAsync<T>(
        HttpSource httpSource,
        string url,
        string cacheKey,
        HttpSourceCacheContext cacheContext,
        Func<Stream, T?> parse,
        CancellationToken cancellationToken
    ) where T : class => httpSource.GetAsync(
        new HttpSourceCachedRequest(url, cacheKey, cacheContext)
        {
            IgnoreNotFounds = true,
        },
        result => Task.FromResult(result.Status is HttpSourceResultStatus.NotFound or HttpSourceResultStatus.NoContent || result.Stream is null
            ? null
            : parse(result.Stream)
        ),
        nugetLogger,
        cancellationToken
    );
}
