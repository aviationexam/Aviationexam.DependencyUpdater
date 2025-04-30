using Aviationexam.DependencyUpdater;
using Aviationexam.DependencyUpdater.ConfigurationParser;
using Aviationexam.DependencyUpdater.DefaultImplementations;
using Aviationexam.DependencyUpdater.Interfaces;
using Aviationexam.DependencyUpdater.Nuget;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;

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
        x => x.MapToNugetFeedAuthentication(envVariableProvider)
    );

    foreach (var nugetUpdate in nugetUpdates)
    {
        await nugetUpdater.ProcessUpdatesAsync(
            directoryPath: Path.Join(directoryPath, nugetUpdate.DirectoryValue.GetString()),
            nugetFeedAuthentications,
            nugetUpdate.TargetFramework is { } targetFramework ? [new NugetTargetFramework(targetFramework.TargetFramework)] : []
        );
    }
}
