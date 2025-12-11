using Aviationexam.DependencyUpdater.Interfaces;
using Aviationexam.DependencyUpdater.Interfaces.Repository;
using Aviationexam.DependencyUpdater.Repository.DevOps;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;

namespace Aviationexam.DependencyUpdater.Commands;

public sealed class AzureDevOpsCommand : Command
{
    private readonly Option<string> _organization;
    private readonly Option<string> _project;
    private readonly Option<string> _repository;
    private readonly Option<string> _pat;
    private readonly Option<string> _accountId;
    private readonly Option<string> _nugetFeedProject;
    private readonly Option<string> _nugetFeedId;
    private readonly Option<string> _serviceHost;
    private readonly Option<string> _accessTokenResourceId;
    private readonly Option<string> _azSideCarAddress;
    private readonly Option<string> _azSideCarToken;

    public AzureDevOpsCommand()
        : base(nameof(EPlatformSelection.AzureDevOps), "Updates dependencies in Azure DevOps repositories.")
    {
        _organization = new Option<string>("--azure-organization")
        {
            Description = "The name of the Azure DevOps organization (e.g., 'contoso').",
            Required = true,
            Arity = ArgumentArity.ExactlyOne,
        };

        _project = new Option<string>("--azure-project")
        {
            Description = "The Azure DevOps project that contains the target repository.",
            Required = true,
            Arity = ArgumentArity.ExactlyOne,
        };

        _repository = new Option<string>("--azure-repository")
        {
            Description = "The name of the Azure DevOps Git repository to operate on.",
            Required = true,
            Arity = ArgumentArity.ExactlyOne,
        };

        _pat = new Option<string>("--azure-pat")
        {
            Description = "The Azure DevOps personal access token used for authentication.",
            Required = true,
            Arity = ArgumentArity.ExactlyOne,
        };

        _accountId = new Option<string>("--azure-account-id")
        {
            Description = "The Azure DevOps user or service account ID used for API access.",
            Required = true,
            Arity = ArgumentArity.ExactlyOne,
        };

        _nugetFeedProject = new Option<string>("--azure-nuget-project")
        {
            Description = "The Azure DevOps project that contains the NuGet artifacts feed.",
            Required = true,
            Arity = ArgumentArity.ExactlyOne,
        };

        _nugetFeedId = new Option<string>("--azure-nuget-feed-id")
        {
            Description = "The ID of the Azure Artifacts NuGet feed used for dependency resolution.",
            Required = true,
            Arity = ArgumentArity.ExactlyOne,
        };

        _serviceHost = new Option<string>("--azure-nuget-service-host")
        {
            Description = "The internal Azure DevOps service host identifier used when accessing upstream package data.",
            Required = true,
            Arity = ArgumentArity.ExactlyOne,
        };

        _accessTokenResourceId = new Option<string>("--azure-access-token-resource-id")
        {
            Description = "The Azure Active Directory resource ID used to acquire access tokens for upstream ingestion.",
            Required = true,
            Arity = ArgumentArity.ExactlyOne,
        };

        _azSideCarAddress = new Option<string>("--azure-az-side-car-address")
        {
            Description = "The URL address for AZ sidecar service, used to fetch Azure DevOps access-token.",
            Required = false,
            Arity = ArgumentArity.ExactlyOne,
        };

        _azSideCarToken = new Option<string>("--azure-az-side-car-token")
        {
            Description = "The token for AZ sidecar service, used to fetch Azure DevOps access-token.",
            Required = false,
            Arity = ArgumentArity.ExactlyOne,
        };

        Options.Add(_organization);
        Options.Add(_project);
        Options.Add(_repository);
        Options.Add(_pat);
        Options.Add(_accountId);
        Options.Add(_nugetFeedProject);
        Options.Add(_nugetFeedId);
        Options.Add(_serviceHost);
        Options.Add(_accessTokenResourceId);
        Options.Add(_azSideCarAddress);
        Options.Add(_azSideCarToken);
    }

    public void ConfigureServices(
        IServiceCollection serviceCollection,
        ParseResult parseResult,
        Option<string> directory,
        Option<string> gitUsername,
        Option<string> gitPassword,
        Option<bool> resetCache
    )
    {
        serviceCollection
            .BindCommonConfiguration(parseResult, directory, gitUsername, gitPassword, resetCache)
            .AddBinder(parseResult, new AzureDevOpsConfigurationBinder(_organization, _project, _repository, _pat, _accountId))
            .AddSingleton<IRepositoryPlatformConfiguration>(x => x.GetRequiredService<AzureDevOpsConfiguration>())
            .AddBinder(parseResult, new AzureDevOpsUndocumentedConfigurationBinder(_nugetFeedProject, _nugetFeedId, _serviceHost, _accessTokenResourceId))
            .AddOptionalBinder(parseResult, new AzCliSideCarConfigurationBinder(_azSideCarAddress, _azSideCarToken));
    }
}
