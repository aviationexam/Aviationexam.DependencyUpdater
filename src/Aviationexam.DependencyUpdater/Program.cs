using Aviationexam.DependencyUpdater;
using Aviationexam.DependencyUpdater.Commands;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.IO;

var directory = new Option<string>(
    "--directory"
)
{
    Description = "The path to the local Git repository where dependencies will be updated.",
    Required = true,
    Arity = ArgumentArity.ExactlyOne,
    DefaultValueFactory = _ => Directory.GetCurrentDirectory(),
}.AcceptLegalFilePathsOnly();

var gitUsernameArgument = new Option<string>(
    "--git-username"
)
{
    Description = "The username used for authenticating with the remote Git repository.",
    Required = false,
    Arity = ArgumentArity.ExactlyOne,
    DefaultValueFactory = _ => string.Empty,
};
var gitPasswordArgument = new Option<string>(
    "--git-password"
)
{
    Description = "The password or personal access token used for authenticating with the remote Git repository.",
    Required = true,
    Arity = ArgumentArity.ExactlyOne,
};

var resetCache = new Option<bool>(
    "--reset-cache"
)
{
    Description = "Clears the internal dependency cache before processing updates.",
    Required = false,
    Arity = ArgumentArity.ExactlyOne,
    DefaultValueFactory = _ => false,
};

var rootCommand = new RootCommand("Dependency updater tool that processes dependency updates based on configuration files.")
{
    directory,
    gitUsernameArgument,
    gitPasswordArgument,
    resetCache,
};

var azureDevOpsCommand = new AzureDevOpsCommand();
var githubCommand = new GitHubCommand();

azureDevOpsCommand.SetAction(DefaultCommandHandler.GetHandler((serviceCollection, parseResult) =>
{
    azureDevOpsCommand.ConfigureServices(serviceCollection, parseResult, directory, gitUsernameArgument, gitPasswordArgument, resetCache);
}));

githubCommand.SetAction(DefaultCommandHandler.GetHandler((serviceCollection, parseResult) =>
{
    githubCommand.ConfigureServices(serviceCollection, parseResult, directory, gitUsernameArgument, gitPasswordArgument, resetCache);
}));

rootCommand.Subcommands.Add(azureDevOpsCommand);
rootCommand.Subcommands.Add(githubCommand);

return await rootCommand.Parse(args).InvokeAsync();
