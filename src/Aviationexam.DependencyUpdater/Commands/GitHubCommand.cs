using Aviationexam.DependencyUpdater.Interfaces;
using Aviationexam.DependencyUpdater.Interfaces.Repository;
using Aviationexam.DependencyUpdater.Repository.GitHub;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;

namespace Aviationexam.DependencyUpdater.Commands;

public sealed class GitHubCommand : Command
{
    private readonly Option<string> _owner = new("--owner")
    {
        Description = "The GitHub repository owner (organization or user account name).",
        Required = true,
        Arity = ArgumentArity.ExactlyOne,
    };

    private readonly Option<string> _repository = new("--repository")
    {
        Description = "The name of the GitHub repository to operate on.",
        Required = true,
        Arity = ArgumentArity.ExactlyOne,
    };

    private readonly Option<string> _token = new("--token")
    {
        Description = "The GitHub personal access token (PAT) used for authentication.",
        Required = true,
        Arity = ArgumentArity.ExactlyOne,
    };

    private readonly Option<bool> _cyclePullRequestOnCreation = new("--cycle-pull-request-on-creation")
    {
        Description = "Close and reopen pull request after creation to trigger CI workflows (workaround for GITHUB_TOKEN limitation).",
        Required = false,
        Arity = ArgumentArity.ZeroOrOne,
        DefaultValueFactory = _ => true,
    };

    public GitHubCommand() : base(
        nameof(EPlatformSelection.GitHub), "Updates dependencies in GitHub repositories."
    )
    {
        Options.Add(_owner);
        Options.Add(_repository);
        Options.Add(_token);
        Options.Add(_cyclePullRequestOnCreation);
    }

    public IServiceCollection ConfigureServices(
        IServiceCollection serviceCollection,
        ParseResult parseResult
    ) => serviceCollection
        .AddBinder(parseResult, new GitHubConfigurationBinder(_owner, _repository, _token, _cyclePullRequestOnCreation))
        .AddSingleton<IRepositoryPlatformConfiguration>(x => x.GetRequiredService<GitHubConfiguration>());
}
