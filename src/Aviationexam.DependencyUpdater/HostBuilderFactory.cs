using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.ConfigurationParser;
using Aviationexam.DependencyUpdater.DefaultImplementations;
using Aviationexam.DependencyUpdater.Nuget;
using Aviationexam.DependencyUpdater.Repository.DevOps;
using Aviationexam.DependencyUpdater.Vcs.Git;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace Aviationexam.DependencyUpdater;

public static class HostBuilderFactory
{
    public static HostApplicationBuilder Create(string[] args, Action<IConfigurationBuilder> configure)
    {
        var configuration = new ConfigurationManager();
        configure(configuration);
        HostApplicationBuilderSettings settings = new()
        {
            Args = args,
            Configuration = configuration,
            ContentRootPath = Environment.CurrentDirectory,
        };

        var builder = Host.CreateEmptyApplicationBuilder(settings);

        builder.Services.AddLogging(x => x.AddConsole());
        builder.Services.AddSingleton<TimeProvider>(_ => TimeProvider.System);
        builder.Services.AddCommon();
        builder.Services.AddConfigurationParser();
        builder.Services.AddNuget();
        builder.Services.AddVcsGit();
        builder.Services.AddRepositoryDevOps();
        builder.Services.AddDefaultImplementations();

        return builder;
    }
}
