using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Interfaces.ReleaseNotes;
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
    IEnumerable<IProjectReleasesProvider> projectReleasesProviders,
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

        if (versions.Count == 0)
        {
            return [];
        }

        var packageNotes = new Dictionary<NuGetVersion, string>();
        var nuspecs = new Dictionary<NuGetVersion, NugetNuspecMetadata?>();

        foreach (var leaf in versions)
        {
            var notes = leaf.ReleaseNotes;

            // The newest version's nuspec is always read: it carries the source repository the project releases come from
            if ((notes is null || leaf == versions[^1]) && packageBaseAddress is not null)
            {
                var nuspec = await FetchNuspecMetadataAsync(httpSource, packageBaseAddress, lowerId, leaf.Version, cacheContext, cancellationToken);
                nuspecs[leaf.Version] = nuspec;
                notes ??= nuspec?.ReleaseNotes;
            }

            if (notes is not null)
            {
                packageNotes[leaf.Version] = notes;
            }
        }

        var repositoryUrl = ResolveRepositoryUrl(versions, nuspecs);

        var projectReleases = await FetchProjectReleasesAsync(packageName, repositoryUrl, cancellationToken);

        return versions
            .AsValueEnumerable()
            .Select(leaf => SelectNotes(packageName, leaf.Version, packageNotes, projectReleases))
            .Where(x => x is not null)
            .Select(x => x!)
            .ToList();
    }

    // The source repository of the newest version wins; project URLs are often documentation sites, so they are only a fallback
    internal static string? ResolveRepositoryUrl(
        IReadOnlyList<NugetRegistrationLeaf> versions,
        IReadOnlyDictionary<NuGetVersion, NugetNuspecMetadata?> nuspecs
    )
    {
        var newestToOldest = versions.AsValueEnumerable().Reverse().ToList();

        return newestToOldest.AsValueEnumerable().Select(x => nuspecs.GetValueOrDefault(x.Version)?.RepositoryUrl).FirstOrDefault(x => x is not null)
               ?? newestToOldest.AsValueEnumerable().Select(x => nuspecs.GetValueOrDefault(x.Version)?.ProjectUrl ?? x.ProjectUrl).FirstOrDefault(x => x is not null);
    }

    // A project release describes the version in full, the nuspec usually carries only a link, so the release wins when both exist
    private static PackageReleaseNote? SelectNotes(
        string packageName,
        NuGetVersion version,
        IReadOnlyDictionary<NuGetVersion, string> packageNotes,
        IReadOnlyDictionary<NuGetVersion, ProjectRelease> projectReleases
    )
    {
        if (projectReleases.TryGetValue(version, out var release) && !string.IsNullOrWhiteSpace(release.Body))
        {
            return new PackageReleaseNote(packageName, version.ToNormalizedString(), release.Body);
        }

        return packageNotes.TryGetValue(version, out var notes)
            ? new PackageReleaseNote(packageName, version.ToNormalizedString(), notes)
            : null;
    }

    private async Task<IReadOnlyDictionary<NuGetVersion, ProjectRelease>> FetchProjectReleasesAsync(
        string packageName,
        string? repositoryUrl,
        CancellationToken cancellationToken
    )
    {
        if (repositoryUrl is null || !Uri.TryCreate(repositoryUrl, UriKind.Absolute, out var repositoryUri))
        {
            return new Dictionary<NuGetVersion, ProjectRelease>();
        }

        foreach (var provider in projectReleasesProviders)
        {
            if (!provider.CanProvide(repositoryUri))
            {
                continue;
            }

            try
            {
                var releases = await provider.GetReleasesAsync(repositoryUri, cancellationToken);

                return ProjectReleaseVersionMatcher.MatchReleases(releases, packageName);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                if (logger.IsEnabled(LogLevel.Warning))
                {
                    logger.LogWarning(ex, "Failed to fetch project releases of {PackageName} from {RepositoryUrl}", packageName, repositoryUrl);
                }
            }
        }

        return new Dictionary<NuGetVersion, ProjectRelease>();
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

    private async Task<NugetNuspecMetadata?> FetchNuspecMetadataAsync(
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
            NugetReleaseNotesParser.ParseNuspecMetadata,
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
