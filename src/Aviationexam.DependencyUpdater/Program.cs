using Aviationexam.DependencyUpdater.ConfigurationParser;
using Aviationexam.DependencyUpdater.DefaultImplementations;
using Aviationexam.DependencyUpdater.Nuget;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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

var nugetUpdater = host.Services.GetRequiredService<NugetUpdater>();
var nugetVersionFetcherFactory = host.Services.GetRequiredService<NugetVersionFetcherFactory>();
var logger = host.Services.GetRequiredService<ILogger<NugetUpdater>>();

var directoryPath = "/opt/asp.net/AviationexamWebV3/Src-V3";
var nugetUpdaterContext = nugetUpdater.CreateContext(
    directoryPath
);

var sourceRepositories = nugetUpdaterContext.NugetConfigurations.ToDictionary(
    x => x,
    nugetVersionFetcherFactory.CreateSourceRepository
);

var dependencies = nugetUpdaterContext.MapSourceToDependency(logger);
foreach (var (dependency, sources) in dependencies)
{
}
