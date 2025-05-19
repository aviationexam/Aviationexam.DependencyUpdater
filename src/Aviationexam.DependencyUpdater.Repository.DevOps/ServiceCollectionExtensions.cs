using Aviationexam.DependencyUpdater.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace Aviationexam.DependencyUpdater.Repository.DevOps;

public static class ServiceCollectionExtensions
{
    [RequiresDynamicCode()]
    [RequiresUnreferencedCode()]
    public static IServiceCollection AddRepositoryDevOps(
        this IServiceCollection services, IConfigurationRoot configuration
    ) => services
        .Configure<DevOpsConfiguration>(configuration.GetSection("DevOps"))
        .AddScoped<IRepositoryClient, RepositoryAzureDevOpsClient>();
}
