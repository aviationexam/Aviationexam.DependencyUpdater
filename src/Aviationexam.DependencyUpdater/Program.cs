using Aviationexam.DependencyUpdater;
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
    DefaultValueFactory = _ => Directory.GetCurrentDirectory(),
}.AcceptLegalFilePathsOnly();

var gitUsernameArgument = new Option<string>(
    "--git-username"
)
{
    Description = "The username used for authenticating with the remote Git repository.",
    Required = false,
    DefaultValueFactory = _ => string.Empty,
};
var gitPasswordArgument = new Option<string>(
    "--git-password"
)
{
    Description = "The password or personal access token used for authenticating with the remote Git repository.",
    Required = true,
};

var organization = new Option<string>(
    "--organization"
)
{
    Description = "The name of the Azure DevOps organization (e.g., 'contoso').",
    Required = true,
};

var project = new Option<string>(
    "--project"
)
{
    Description = "The Azure DevOps project that contains the target repository.",
    Required = true,
};

var repository = new Option<string>(
    "--repository"
)
{
    Description = "The name of the Azure DevOps Git repository to operate on.",
    Required = true,
};

var pat = new Option<string>(
    "--pat"
)
{
    Description = "The Azure DevOps personal access token used for authentication.",
    Required = true,
};

var accountId = new Option<string>(
    "--account-id"
)
{
    Description = "The Azure DevOps user or service account ID used for API access.",
    Required = true,
};

var nugetFeedProject = new Option<string>(
    "--nuget-project"
)
{
    Description = "The Azure DevOps project that contains the NuGet artifacts feed.",
    Required = true,
};

var nugetFeedId = new Option<string>(
    "--nuget-feed-id"
)
{
    Description = "The ID of the Azure Artifacts NuGet feed used for dependency resolution.",
    Required = true,
};

var serviceHost = new Option<string>(
    "--nuget-service-host"
)
{
    Description = "The internal Azure DevOps service host identifier used when accessing upstream package data.",
    Required = true,
};

var accessTokenResourceId = new Option<string>(
    "--access-token-resource-id"
)
{
    Description = "The Azure Active Directory resource ID used to acquire access tokens for upstream ingestion.",
    Required = true,
};
var azSideCarAddress = new Option<string>(
    "--az-side-car-address"
)
{
    Description = "The URL address for AZ sidecar service, used to fetch Azure DevOps access-token.",
    Required = false,
};
var azSideCarToken = new Option<string>(
    "--az-side-car-token"
)
{
    Description = "The token for AZ sidecar service, used to fetch Azure DevOps access-token.",
    Required = false,
};

var resetCache = new Option<bool>(
    "--reset-cache"
)
{
    Description = "Clears the internal dependency cache before processing updates.",
    Required = false,
    DefaultValueFactory = _ => false
};

var rootCommand = new RootCommand("Dependency updater tool that processes dependency updates based on configuration files.")
{
    directory,
    gitUsernameArgument,
    gitPasswordArgument,
    organization,
    project,
    repository,
    pat,
    accountId,
    nugetFeedProject,
    nugetFeedId,
    serviceHost,
    accessTokenResourceId,
    azSideCarAddress,
    azSideCarToken,
    resetCache,
};
rootCommand.SetAction(DefaultCommandHandler.GetHandler((serviceCollection, parseResult) => serviceCollection
    .AddBinder(parseResult, new SourceConfigurationBinder(directory))
    .AddBinder(parseResult, new GitCredentialsConfigurationBinder(gitUsernameArgument, gitPasswordArgument))
    .AddBinder(parseResult, new DevOpsConfigurationBinder(organization, project, repository, pat, accountId))
    .AddBinder(parseResult, new DevOpsUndocumentedConfigurationBinder(nugetFeedProject, nugetFeedId, serviceHost, accessTokenResourceId))
    .AddOptionalBinder(parseResult, new AzCliSideCarConfigurationBinder(azSideCarAddress, azSideCarToken))
    .AddBinder(parseResult, x => new CachingConfigurationBinder(
        x.GetRequiredService<TimeProvider>(),
        resetCache
    ))
));

return await rootCommand.Parse(args).InvokeAsync();
