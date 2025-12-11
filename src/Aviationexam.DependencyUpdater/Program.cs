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

var platform = new Option<string>(
    "--platform"
)
{
    Description = "The repository platform to use: azure-devops or github",
    Required = true,
};

var azureOrganization = new Option<string>(
    "--azure-organization"
)
{
    Description = "The name of the Azure DevOps organization (e.g., 'contoso').",
    Required = true,
};

var azureProject = new Option<string>(
    "--azure-project"
)
{
    Description = "The Azure DevOps project that contains the target repository.",
    Required = true,
};

var azureRepository = new Option<string>(
    "--azure-repository"
)
{
    Description = "The name of the Azure DevOps Git repository to operate on.",
    Required = true,
};

var azurePat = new Option<string>(
    "--azure-pat"
)
{
    Description = "The Azure DevOps personal access token used for authentication.",
    Required = true,
};

var azureAccountId = new Option<string>(
    "--azure-account-id"
)
{
    Description = "The Azure DevOps user or service account ID used for API access.",
    Required = true,
};

var azureNugetFeedProject = new Option<string>(
    "--azure-nuget-project"
)
{
    Description = "The Azure DevOps project that contains the NuGet artifacts feed.",
    Required = true,
};

var azureNugetFeedId = new Option<string>(
    "--azure-nuget-feed-id"
)
{
    Description = "The ID of the Azure Artifacts NuGet feed used for dependency resolution.",
    Required = true,
};

var azureServiceHost = new Option<string>(
    "--azure-nuget-service-host"
)
{
    Description = "The internal Azure DevOps service host identifier used when accessing upstream package data.",
    Required = true,
};

var azureAccessTokenResourceId = new Option<string>(
    "--azure-access-token-resource-id"
)
{
    Description = "The Azure Active Directory resource ID used to acquire access tokens for upstream ingestion.",
    Required = true,
};

var azureAzSideCarAddress = new Option<string>(
    "--azure-az-side-car-address"
)
{
    Description = "The URL address for AZ sidecar service, used to fetch Azure DevOps access-token.",
    Required = false,
};

var azureAzSideCarToken = new Option<string>(
    "--azure-az-side-car-token"
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
    platform,
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
    resetCache,
};
rootCommand.SetAction(DefaultCommandHandler.GetHandler((serviceCollection, parseResult) =>
{
    // Parse and register PlatformSelection manually (enum can't use IBinder<T>)
    var platformValue = parseResult.GetRequiredValue(platform);
    var platformSelection = platformValue.ToLowerInvariant() switch
    {
        "azure-devops" => EPlatformSelection.AzureDevOps,
        "github" => EPlatformSelection.GitHub,
        _ => throw new ArgumentException($"Unknown platform: {platformValue}. Valid values are: azure-devops, github")
    };

    serviceCollection.AddSingleton(typeof(EPlatformSelection), _ => platformSelection);

    serviceCollection
        .AddBinder(parseResult, new SourceConfigurationBinder(directory))
        .AddBinder(parseResult, new GitCredentialsConfigurationBinder(gitUsernameArgument, gitPasswordArgument))
        .AddBinder(parseResult, new AzureDevOpsConfigurationBinder(azureOrganization, azureProject, azureRepository, azurePat, azureAccountId))
        .AddBinder(parseResult, new AzureDevOpsUndocumentedConfigurationBinder(azureNugetFeedProject, azureNugetFeedId, azureServiceHost, azureAccessTokenResourceId))
        .AddOptionalBinder(parseResult, new AzCliSideCarConfigurationBinder(azureAzSideCarAddress, azureAzSideCarToken))
        .AddBinder(parseResult, x => new CachingConfigurationBinder(
            x.GetRequiredService<TimeProvider>(),
            resetCache
        ));
}));

return await rootCommand.Parse(args).InvokeAsync();
