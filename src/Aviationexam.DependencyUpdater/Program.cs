using Aviationexam.DependencyUpdater;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.IO;

var directory = new Option<string>(
    "--directory",
    description: "The path to the local Git repository where dependencies will be updated.",
    getDefaultValue: Directory.GetCurrentDirectory
)
{
    IsRequired = true,
};

var gitUsernameArgument = new Option<string>(
    "--git-username",
    description: "The username used for authenticating with the remote Git repository.",
    getDefaultValue: () => string.Empty
)
{
    IsRequired = false,
};
var gitPasswordArgument = new Option<string>(
    "--git-password",
    description: "The password or personal access token used for authenticating with the remote Git repository."
)
{
    IsRequired = true,
};

var organization = new Option<string>(
    "--organization",
    description: "The name of the Azure DevOps organization (e.g., 'contoso')."
)
{
    IsRequired = true,
};

var project = new Option<string>(
    "--project",
    description: "The Azure DevOps project that contains the target repository."
)
{
    IsRequired = true,
};

var repository = new Option<string>(
    "--repository",
    description: "The name of the Azure DevOps Git repository to operate on."
)
{
    IsRequired = true,
};

var pat = new Option<string>(
    "--pat",
    description: "The Azure DevOps personal access token used for authentication."
)
{
    IsRequired = true,
};

var accountId = new Option<string>(
    "--account-id",
    description: "The Azure DevOps user or service account ID used for API access."
)
{
    IsRequired = true,
};

var nugetFeedProject = new Option<string>(
    "--nuget-project",
    description: "The Azure DevOps project that contains the NuGet artifacts feed."
)
{
    IsRequired = true,
};

var nugetFeedId = new Option<string>(
    "--nuget-feed-id",
    description: "The ID of the Azure Artifacts NuGet feed used for dependency resolution."
)
{
    IsRequired = true,
};

var serviceHost = new Option<string>(
    "--nuget-service-host",
    description: "The internal Azure DevOps service host identifier used when accessing upstream package data."
)
{
    IsRequired = true,
};

var accessTokenResourceId = new Option<string>(
    "--access-token-resource-id",
    description: "The Azure Active Directory resource ID used to acquire access tokens for upstream ingestion."
)
{
    IsRequired = true,
};
var azSideCarAddress = new Option<string>(
    "--az-side-car-address",
    description: "The URL address for AZ sidecar service, used to fetch Azure DevOps access-token."
)
{
    IsRequired = false,
};
var azSideCarToken = new Option<string>(
    "--az-side-car-token",
    description: "The token for AZ sidecar service, used to fetch Azure DevOps access-token."
)
{
    IsRequired = false,
};

var resetCache = new Option<bool>(
    "--reset-cache",
    description: "Clears the internal dependency cache before processing updates.",
    getDefaultValue: () => false
)
{
    IsRequired = false,
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
rootCommand.Handler = DefaultCommandHandler.GetHandler();

return await new CommandLineBuilder(rootCommand)
    .UseHost(HostBuilderFactory.Create, (hostApplicationBuilder, modelBinder) =>
    {
        hostApplicationBuilder.Services.AddBinder(modelBinder, _ => new SourceConfigurationBinder(directory));
        hostApplicationBuilder.Services.AddBinder(modelBinder, _ => new GitCredentialsConfigurationBinder(gitUsernameArgument, gitPasswordArgument));
        hostApplicationBuilder.Services.AddBinder(modelBinder, _ => new DevOpsConfigurationBinder(organization, project, repository, pat, accountId));
        hostApplicationBuilder.Services.AddBinder(modelBinder, _ => new DevOpsUndocumentedConfigurationBinder(nugetFeedProject, nugetFeedId, serviceHost, accessTokenResourceId));
        hostApplicationBuilder.Services.AddOptionalBinder(modelBinder, _ => new AzCliSideCarConfigurationBinder(azSideCarAddress, azSideCarToken));
        hostApplicationBuilder.Services.AddBinder(modelBinder, x => new CachingConfigurationBinder(
            x.GetRequiredService<TimeProvider>(),
            resetCache
        ));
    })
    .UseDefaults()
    .Build()
    .InvokeAsync(args);
