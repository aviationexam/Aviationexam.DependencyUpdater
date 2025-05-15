using Aviationexam.DependencyUpdater.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Aviationexam.DependencyUpdater.Repository.DevOps;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRepositoryDevOps(
        this IServiceCollection services, IConfigurationRoot configuration
    ) => services
        .Configure<DevOpsConfiguration>(configuration.GetSection("DevOps"))
        .AddScoped<IRepositoryClient, RepositoryAzureDevOpsClient>();
}
