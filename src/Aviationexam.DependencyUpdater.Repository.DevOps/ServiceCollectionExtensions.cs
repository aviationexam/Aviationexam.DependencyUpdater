using Aviationexam.DependencyUpdater.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace Aviationexam.DependencyUpdater.Repository.DevOps;

public static class ServiceCollectionExtensions
{
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Safe due to source-generated binding")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Safe due to source-generated binding")]
    public static IServiceCollection AddRepositoryDevOps(
        this IServiceCollection services, IConfigurationRoot configuration
    ) => services
        .Configure<DevOpsConfiguration>(configuration.GetSection("DevOps"))
        .AddScoped<IRepositoryClient, RepositoryAzureDevOpsClient>();
}
