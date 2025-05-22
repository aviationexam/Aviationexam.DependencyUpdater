using Aviationexam.DependencyUpdater;
using System.CommandLine;
using System.IO;

var directory = new Option<string>("--directory", description: "The directory containing the repository to update dependencies in", getDefaultValue: Directory.GetCurrentDirectory)
{
    IsRequired = true,
};
var organization = new Option<string>("--organization") { IsRequired = true };
var project = new Option<string>("--project") { IsRequired = true };
var repository = new Option<string>("--repository") { IsRequired = true };
var pat = new Option<string>("--pat") { IsRequired = true };
var accountId = new Option<string>("--account-id") { IsRequired = true };
var nugetFeedId = new Option<string>("--nuget-feed-id") { IsRequired = true };
var serviceHost = new Option<string>("--nuget-service-host") { IsRequired = true };
var accessTokenResourceId = new Option<string>("--access-token-resource-id") { IsRequired = true };

var rootCommand = new RootCommand("Dependency updater tool that processes dependency updates based on configuration files.")
{
    directory,
    organization,
    project,
    repository,
    pat,
    accountId,
    nugetFeedId,
    serviceHost,
    accessTokenResourceId,
};

rootCommand.SetHandler(
    async (sourceConfiguration, devOpsConfiguration, devOpsUndocumentedConfiguration) =>
    {
        using var host = DefaultCommandHandler.CreateHostBuilder(args, sourceConfiguration, devOpsConfiguration, devOpsUndocumentedConfiguration);
        await DefaultCommandHandler.ExecuteWithBuilderAsync(host.Services);
    },
    new SourceConfigurationBinder(directory),
    new DevOpsConfigurationBinder(organization, project, repository, pat, accountId),
    new DevOpsUndocumentedConfigurationBinder(nugetFeedId, serviceHost, accessTokenResourceId)
);

return await rootCommand.InvokeAsync(args);
