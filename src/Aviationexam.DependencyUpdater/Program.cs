using Aviationexam.DependencyUpdater;
using Aviationexam.DependencyUpdater.Interfaces;
using Aviationexam.DependencyUpdater.Interfaces.Repository;
using Aviationexam.DependencyUpdater.Repository.DevOps;
using Microsoft.Extensions.DependencyInjection;
using System;
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

var azureOrganization = new Option<string>(
    "--azure-organization"
)
{
    Description = "The name of the Azure DevOps organization (e.g., 'contoso').",
    Required = true,
    Arity = ArgumentArity.ExactlyOne,
};

var azureProject = new Option<string>(
    "--azure-project"
)
{
    Description = "The Azure DevOps project that contains the target repository.",
    Required = true,
    Arity = ArgumentArity.ExactlyOne,
};

var azureRepository = new Option<string>(
    "--azure-repository"
)
{
    Description = "The name of the Azure DevOps Git repository to operate on.",
    Required = true,
    Arity = ArgumentArity.ExactlyOne,
};

var azurePat = new Option<string>(
    "--azure-pat"
)
{
    Description = "The Azure DevOps personal access token used for authentication.",
    Required = true,
    Arity = ArgumentArity.ExactlyOne,
};

var azureAccountId = new Option<string>(
    "--azure-account-id"
)
{
    Description = "The Azure DevOps user or service account ID used for API access.",
    Required = true,
    Arity = ArgumentArity.ExactlyOne,
};

var azureNugetFeedProject = new Option<string>(
    "--azure-nuget-project"
)
{
    Description = "The Azure DevOps project that contains the NuGet artifacts feed.",
    Required = true,
    Arity = ArgumentArity.ExactlyOne,
};

var azureNugetFeedId = new Option<string>(
    "--azure-nuget-feed-id"
)
{
    Description = "The ID of the Azure Artifacts NuGet feed used for dependency resolution.",
    Required = true,
    Arity = ArgumentArity.ExactlyOne,
};

var azureServiceHost = new Option<string>(
    "--azure-nuget-service-host"
)
{
    Description = "The internal Azure DevOps service host identifier used when accessing upstream package data.",
    Required = true,
    Arity = ArgumentArity.ExactlyOne,
};

var azureAccessTokenResourceId = new Option<string>(
    "--azure-access-token-resource-id"
)
{
    Description = "The Azure Active Directory resource ID used to acquire access tokens for upstream ingestion.",
    Required = true,
    Arity = ArgumentArity.ExactlyOne,
};

var azureAzSideCarAddress = new Option<string>(
    "--azure-az-side-car-address"
)
{
    Description = "The URL address for AZ sidecar service, used to fetch Azure DevOps access-token.",
    Required = false,
    Arity = ArgumentArity.ExactlyOne,
};

var azureAzSideCarToken = new Option<string>(
    "--azure-az-side-car-token"
)
{
    Description = "The token for AZ sidecar service, used to fetch Azure DevOps access-token.",
    Required = false,
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

var azureDevOpsCommand = new Command(nameof(EPlatformSelection.AzureDevOps), "Updates dependencies in Azure DevOps repositories.")
{
    azureOrganization,
    azureProject,
    azureRepository,
    azurePat,
    azureAccountId,
    azureNugetFeedProject,
    azureNugetFeedId,
    azureServiceHost,
    azureAccessTokenResourceId,
    azureAzSideCarAddress,
    azureAzSideCarToken,
};
var githubCommand = new Command(nameof(EPlatformSelection.GitHub), "Updates dependencies in GitHub repositories.")
{
};

azureDevOpsCommand.SetAction(DefaultCommandHandler.GetHandler((serviceCollection, parseResult) => serviceCollection
    .AddBinder(parseResult, new SourceConfigurationBinder(directory))
    .AddBinder(parseResult, new GitCredentialsConfigurationBinder(gitUsernameArgument, gitPasswordArgument))
    .AddBinder(parseResult, x => new CachingConfigurationBinder(
        x.GetRequiredService<TimeProvider>(),
        resetCache
    ))
    .AddBinder(parseResult, new AzureDevOpsConfigurationBinder(azureOrganization, azureProject, azureRepository, azurePat, azureAccountId))
    .AddSingleton<IRepositoryPlatformConfiguration>(x => x.GetRequiredService<AzureDevOpsConfiguration>())
    .AddBinder(parseResult, new AzureDevOpsUndocumentedConfigurationBinder(azureNugetFeedProject, azureNugetFeedId, azureServiceHost, azureAccessTokenResourceId))
    .AddOptionalBinder(parseResult, new AzCliSideCarConfigurationBinder(azureAzSideCarAddress, azureAzSideCarToken))
));
githubCommand.SetAction(DefaultCommandHandler.GetHandler((serviceCollection, parseResult) => serviceCollection
    .AddBinder(parseResult, new SourceConfigurationBinder(directory))
    .AddBinder(parseResult, new GitCredentialsConfigurationBinder(gitUsernameArgument, gitPasswordArgument))
    .AddBinder(parseResult, x => new CachingConfigurationBinder(
        x.GetRequiredService<TimeProvider>(),
        resetCache
    ))
));

rootCommand.Subcommands.Add(azureDevOpsCommand);
rootCommand.Subcommands.Add(githubCommand);

return await rootCommand.Parse(args).InvokeAsync();
