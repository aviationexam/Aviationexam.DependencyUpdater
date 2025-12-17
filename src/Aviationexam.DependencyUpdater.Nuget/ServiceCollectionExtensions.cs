using Aviationexam.DependencyUpdater.Nuget.Factories;
using Aviationexam.DependencyUpdater.Nuget.Filtering;
using Aviationexam.DependencyUpdater.Nuget.Grouping;
using Aviationexam.DependencyUpdater.Nuget.Parsers;
using Aviationexam.DependencyUpdater.Nuget.Services;
using Aviationexam.DependencyUpdater.Nuget.Writers;
using Microsoft.Extensions.DependencyInjection;
using NuGet.Common;
using NuGet.Protocol.Core.Types;

namespace Aviationexam.DependencyUpdater.Nuget;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNuget(
        this IServiceCollection services
    ) => services
        .AddScoped<NugetFinder>()
        .AddScoped<NugetConfigParser>()
        .AddScoped<NugetCsprojParser>()
        .AddScoped<NugetDirectoryPackagesPropsParser>()
        .AddScoped<DotnetToolsParser>()
        .AddScoped<NugetUpdater>()
        .AddScoped<DependencyAnalyzer>()
        .AddScoped<NugetContextFactory>()
        .AddScoped<PackageFilterer>()
        .AddScoped<PackageUpdater>()
        .AddScoped<PackageGrouper>()
        .AddScoped<PullRequestManager>()
        .AddScoped<SubmoduleUpdater>()
        .AddSingleton<Repository.RepositoryFactory>(_ => Repository.Factory)
        .AddScoped<ILogger, NuGetLoggerAdapter>()
        .AddScoped<NugetVersionFetcherFactory>()
        .AddScoped<INugetVersionFetcher, NugetVersionFetcher>()
        .AddScoped<TargetFrameworksResolver>()
        .AddScoped<IgnoredDependenciesResolver>()
        .AddScoped<NugetVersionWriter>()
        .AddScoped<NugetCsprojVersionWriter>()
        .AddScoped<NugetDirectoryPackagesPropsVersionWriter>()
        .AddScoped<DotnetToolsVersionWriter>()
        .AddScoped<NugetCli>();
}
