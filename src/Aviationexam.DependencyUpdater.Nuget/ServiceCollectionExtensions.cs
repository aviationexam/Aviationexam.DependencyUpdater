using Microsoft.Extensions.DependencyInjection;
using NuGet.Protocol.Core.Types;

namespace Aviationexam.DependencyUpdater.Nuget;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNuget(
        this IServiceCollection services
    ) => services
        .AddScoped<NugetFinder>()
        .AddScoped<NugetConfigParser>()
        .AddScoped<CsprojParser>()
        .AddScoped<DirectoryPackagesPropsParser>()
        .AddScoped<NugetUpdater>()
        .AddSingleton<Repository.RepositoryFactory>(_ => Repository.Factory)
        .AddScoped<NuGet.Common.ILogger, NuGetLoggerAdapter>()
        .AddScoped<NugetVersionFetcherFactory>()
        .AddScoped<NugetVersionFetcher>();
}
