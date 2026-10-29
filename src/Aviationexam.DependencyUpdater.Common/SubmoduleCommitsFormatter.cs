using Aviationexam.DependencyUpdater.Interfaces;
using System;
using System.Text;
using System.Text.RegularExpressions;
using ZLinq;

namespace Aviationexam.DependencyUpdater.Common;

public static partial class SubmoduleCommitsFormatter
{
    public const string SectionHeader = "## Commits";
    public const string TruncationNotice = "_The commit list was truncated to fit the pull request description limit._";
    public const int MaxCommits = 10;

    [GeneratedRegex(@"(?<=[^\w.-])(?<char>[_*`~\[\]<>])", RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 100)]
    private static partial Regex MarkdownCharactersRegexFactory();

    private static readonly Regex MarkdownCharacters = MarkdownCharactersRegexFactory();

    public static string? BuildSection(
        SubmoduleUpdate submoduleUpdate
    )
    {
        if (submoduleUpdate.Commits.Count == 0)
        {
            return null;
        }

        var repositoryUrl = GetRepositoryWebUrl(submoduleUpdate.RemoteUrl);
        var compareUrl = GetCompareUrl(repositoryUrl, submoduleUpdate.FromCommit, submoduleUpdate.ToCommit);

        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine(SectionHeader);
        stringBuilder.AppendLine();

        foreach (var commit in submoduleUpdate.Commits.AsValueEnumerable().Take(MaxCommits))
        {
            var shortSha = commit.Sha[..Math.Min(7, commit.Sha.Length)];
            var commitUrl = GetCommitUrl(repositoryUrl, commit.Sha);
            var shaText = commitUrl is null ? $"`{shortSha}`" : $"[`{shortSha}`]({commitUrl})";

            stringBuilder.Append("- ").Append(shaText).Append(' ').AppendLine(GetTitle(commit.Message));
        }

        if (compareUrl is not null)
        {
            stringBuilder.Append("- ").AppendLine(submoduleUpdate.Commits.Count > MaxCommits
                ? $"Additional commits viewable in [compare view]({compareUrl})"
                : $"See full diff in [compare view]({compareUrl})"
            );
        }
        else if (submoduleUpdate.Commits.Count > MaxCommits)
        {
            stringBuilder.Append("- ").AppendLine($"… and {submoduleUpdate.Commits.Count - MaxCommits} more commits");
        }

        return stringBuilder.ToString().TrimEnd();
    }

    public static string GetTitle(
        string message
    )
    {
        var title = message.ReplaceLineEndings("\n").Split('\n')[0].Trim();

        return ReleaseNotesFormatter.ScrubMentionsAndIssueReferences(MarkdownCharacters.Replace(title, "\\${char}"));
    }

    public static Uri? GetRepositoryWebUrl(
        string? remoteUrl
    )
    {
        if (string.IsNullOrWhiteSpace(remoteUrl))
        {
            return null;
        }

        // git@host:owner/repo.git
        var scpMatch = ScpLikeUrl.Match(remoteUrl);
        if (scpMatch.Success)
        {
            remoteUrl = $"https://{scpMatch.Groups["host"].Value}/{scpMatch.Groups["path"].Value}";
        }

        if (!Uri.TryCreate(remoteUrl, UriKind.Absolute, out var uri) || uri.Scheme is not ("https" or "http" or "ssh" or "git"))
        {
            return null;
        }

        var host = uri.Host;
        var path = uri.AbsolutePath.TrimEnd('/');
        if (path.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
        {
            path = path[..^4];
        }

        // git@ssh.dev.azure.com:v3/{organization}/{project}/{repository}
        if (host.Equals("ssh.dev.azure.com", StringComparison.OrdinalIgnoreCase))
        {
            var segments = path.Trim('/').Split('/');
            if (segments is ["v3", var organization, var project, var repository])
            {
                host = "dev.azure.com";
                path = $"/{organization}/{project}/_git/{repository}";
            }
        }

        // HTTP(S) remotes keep their scheme and port, SSH and git remotes map to the HTTPS web endpoint
        var isHttp = uri.Scheme is "https" or "http";

        return new UriBuilder(isHttp ? uri.Scheme : Uri.UriSchemeHttps, host, isHttp ? uri.Port : -1) { Path = path }.Uri;
    }

    [GeneratedRegex(@"^(?:[\w.-]+@)?(?<host>[\w.-]+):(?!//)(?<path>[^:]+)$", RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 100)]
    private static partial Regex ScpLikeUrlRegexFactory();

    private static readonly Regex ScpLikeUrl = ScpLikeUrlRegexFactory();

    // GitHub, GitLab, Bitbucket and Azure DevOps all serve commits at /commit/{sha}
    public static string? GetCommitUrl(
        Uri? repositoryUrl,
        string sha
    ) => repositoryUrl is null ? null : $"{Escaped(repositoryUrl)}/commit/{sha}";

    // Uri.ToString() unescapes "%20" and alike, which breaks Markdown link destinations
    private static string Escaped(
        Uri repositoryUrl
    ) => repositoryUrl.AbsoluteUri.TrimEnd('/');

    public static string? GetCompareUrl(
        Uri? repositoryUrl,
        string? fromCommit,
        string toCommit
    )
    {
        if (repositoryUrl is null)
        {
            return null;
        }

        var url = Escaped(repositoryUrl);

        if (IsAzureDevOps(repositoryUrl))
        {
            return fromCommit is null
                ? $"{url}/commits?itemVersion=GC{toCommit}"
                : $"{url}/branchCompare?baseVersion=GC{fromCommit}&targetVersion=GC{toCommit}&_a=commits";
        }

        return fromCommit is null
            ? $"{url}/commits/{toCommit}"
            : $"{url}/compare/{fromCommit}...{toCommit}";
    }

    private static bool IsAzureDevOps(
        Uri repositoryUrl
    ) => repositoryUrl.Host.Equals("dev.azure.com", StringComparison.OrdinalIgnoreCase)
         || repositoryUrl.Host.EndsWith(".visualstudio.com", StringComparison.OrdinalIgnoreCase);
}
