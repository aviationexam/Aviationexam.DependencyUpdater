using Aviationexam.DependencyUpdater.Interfaces;
using LibGit2Sharp;

namespace Aviationexam.DependencyUpdater.Vcs.Git.Extensions;

public static class GitCredentialsConfigurationExtensions
{
    public static Credentials ToGitCredentials(
        this GitCredentialsConfiguration gitCredentialsConfiguration
    ) => new UsernamePasswordCredentials
    {
        Username = gitCredentialsConfiguration.Username,
        Password = gitCredentialsConfiguration.Password,
    };
}
