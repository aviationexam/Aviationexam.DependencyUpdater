using Aviationexam.DependencyUpdater.Interfaces.ReleaseNotes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Octokit;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ZLinq;

namespace Aviationexam.DependencyUpdater.Repository.GitHub;

public sealed class GitHubProjectReleasesProvider(
    [FromKeyedServices(ServiceCollectionExtensions.ProjectReleases)]
    IGitHubClient gitHubClient,
    ILogger<GitHubProjectReleasesProvider> logger
) : IProjectReleasesProvider
{
    // Matches dependabot, which reads a single page of the newest 100 releases
    public const int MaxReleases = 100;

    private readonly ConcurrentDictionary<string, Task<IReadOnlyCollection<ProjectRelease>>> _cache = new(StringComparer.OrdinalIgnoreCase);

    public bool CanProvide(
        Uri repositoryUrl
    ) => GitHubRepositoryUrl.TryParse(repositoryUrl, out _, out _);

    public Task<IReadOnlyCollection<ProjectRelease>> GetReleasesAsync(
        Uri repositoryUrl,
        CancellationToken cancellationToken
    )
    {
        if (!GitHubRepositoryUrl.TryParse(repositoryUrl, out var owner, out var repository))
        {
            return Task.FromResult<IReadOnlyCollection<ProjectRelease>>([]);
        }

        return _cache.GetOrAdd($"{owner}/{repository}", _ => FetchReleasesAsync(owner, repository, cancellationToken));
    }

    private async Task<IReadOnlyCollection<ProjectRelease>> FetchReleasesAsync(
        string owner,
        string repository,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var releases = await gitHubClient.Repository.Release.GetAll(owner, repository, new ApiOptions
            {
                PageSize = MaxReleases,
                PageCount = 1,
            }).WaitAsync(cancellationToken);

            return releases
                .AsValueEnumerable()
                .Where(x => !x.Draft)
                .Select(x => new ProjectRelease(x.Name, x.TagName, x.Body))
                .ToList();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning(ex, "Failed to fetch GitHub releases for {Owner}/{Repository}", owner, repository);
            }

            return [];
        }
    }
}
