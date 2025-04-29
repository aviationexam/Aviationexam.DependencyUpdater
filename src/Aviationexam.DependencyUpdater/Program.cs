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
builder.Services.AddScoped<NugetFinder>();
builder.Services.AddScoped<CsprojParser>();
builder.Services.AddScoped<DirectoryPackagesPropsParser>();
builder.Services.AddScoped<NugetUpdater>();
builder.Services.AddScoped<IFileSystem, FileSystem>();

using var host = builder.Build();

var nugetUpdater = host.Services.GetRequiredService<NugetUpdater>();

var nugetUpdaterContext = nugetUpdater.CreateContext(
    directoryPath: "/opt/asp.net/AviationexamWebV3/Src-V3"
);

