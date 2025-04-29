using Aviationexam.DependencyUpdater.ConfigurationParser;
using Aviationexam.DependencyUpdater.DefaultImplementations;
using Aviationexam.DependencyUpdater.Interfaces;
using Aviationexam.DependencyUpdater.Nuget;
using Corvus.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;

HostApplicationBuilderSettings settings = new()
{
    Args = args,
    Configuration = new ConfigurationManager(),
    ContentRootPath = Directory.GetCurrentDirectory(),
};

var builder = Host.CreateEmptyApplicationBuilder(settings);

builder.Services.AddLogging(x => x.AddConsole());

builder.Services.AddConfigurationParser();
builder.Services.AddNuget();
builder.Services.AddDefaultImplementations();

using var host = builder.Build();

var dependabotConfigurationLoader = host.Services.GetRequiredService<DependabotConfigurationLoader>();
var envVariableProvider = host.Services.GetRequiredService<IEnvVariableProvider>();
var nugetUpdater = host.Services.GetRequiredService<NugetUpdater>();
var nugetVersionFetcherFactory = host.Services.GetRequiredService<NugetVersionFetcherFactory>();
var logger = host.Services.GetRequiredService<ILogger<NugetUpdater>>();

var directoryPath = "/opt/asp.net/AviationexamWebV3/";
var dependabotConfigurations = dependabotConfigurationLoader.LoadConfiguration(directoryPath);

foreach (var dependabotConfiguration in dependabotConfigurations)
{
    var nugetUpdates = dependabotConfiguration.ExtractEcosystemUpdates("nuget");

    if (nugetUpdates.Count == 0)
    {
        continue;
    }

    var nugetFeedAuthentications = dependabotConfiguration.ExtractFeeds(
        "nuget-feed",
        x => NugetFeedAuthenticationFactory.CreateNugetFeedAuthentication(
            envVariableProvider,
            x.Url.GetString()!,
            x.Username.GetString(),
            x.Password.GetString(),
            x.Token.GetString()
        )
    );

    foreach (var nugetUpdate in nugetUpdates)
    {
        var directory = nugetUpdate.DirectoryValue.GetString();

        var nugetUpdaterContext = nugetUpdater.CreateContext(
            Path.Join(directoryPath, directory)
        );

        var sourceRepositories = nugetUpdaterContext.NugetConfigurations.ToDictionary(
            x => x,
            x => nugetVersionFetcherFactory.CreateSourceRepository(x, nugetFeedAuthentications)
        );

        var dependencies = nugetUpdaterContext.MapSourceToDependency(logger);
        foreach (var (dependency, sources) in dependencies)
        {
        }
    }
}
