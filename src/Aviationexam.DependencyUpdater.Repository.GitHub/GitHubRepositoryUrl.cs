using System;
using System.Diagnostics.CodeAnalysis;

namespace Aviationexam.DependencyUpdater.Repository.GitHub;

public static class GitHubRepositoryUrl
{
    public static bool TryParse(
        Uri repositoryUrl,
        [NotNullWhen(true)] out string? owner,
        [NotNullWhen(true)] out string? repository
    )
    {
        owner = null;
        repository = null;

        if (
            !repositoryUrl.IsAbsoluteUri
            || repositoryUrl.Scheme is not ("https" or "http")
            || repositoryUrl.Host is not ("github.com" or "www.github.com")
        )
        {
            return false;
        }

        var segments = repositoryUrl.AbsolutePath.Trim('/').Split('/');
        if (segments.Length < 2 || segments[0].Length == 0 || segments[1].Length == 0)
        {
            return false;
        }

        owner = segments[0];
        repository = segments[1].EndsWith(".git", StringComparison.OrdinalIgnoreCase)
            ? segments[1][..^4]
            : segments[1];

        return repository.Length > 0;
    }
}
