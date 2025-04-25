using Aviationexam.DependencyUpdater.DefaultImplementations;
using Aviationexam.DependencyUpdater.Interfaces;
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
builder.Services.AddScoped<NugetFinder>();
builder.Services.AddScoped<CsprojParser>();
builder.Services.AddScoped<DirectoryPackagesPropsParser>();
builder.Services.AddScoped<DependencyParser>();
builder.Services.AddScoped<IFileSystem, FileSystem>();

using var host = builder.Build();

var nugetFinder = host.Services.GetRequiredService<NugetFinder>();
var dependencyParser = host.Services.GetRequiredService<DependencyParser>();

var dependencies = dependencyParser.GetAllDependencies(nugetFinder.GetAllNugetFiles(
    directoryPath: "/opt/asp.net/AviationexamWebV3/Src-V3"
));

var a = dependencies.ToList();

var b = a;
