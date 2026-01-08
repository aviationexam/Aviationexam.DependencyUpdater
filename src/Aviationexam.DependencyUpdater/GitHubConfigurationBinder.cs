using Aviationexam.DependencyUpdater.Repository.GitHub;
using System.CommandLine;

namespace Aviationexam.DependencyUpdater;

public sealed class GitHubConfigurationBinder(
    Option<string> owner,
    Option<string> repository,
    Option<string> token,
    Option<bool> cyclePullRequestOnCreation
) : IBinder<GitHubConfiguration>
{
    public GitHubConfiguration CreateValue(
        ParseResult parseResult
    ) => new()
    {
        Owner = parseResult.GetRequiredValue(owner),
        Repository = parseResult.GetRequiredValue(repository),
        Token = parseResult.GetRequiredValue(token),
        CyclePullRequestOnCreation = parseResult.GetValue(cyclePullRequestOnCreation),
    };
}
