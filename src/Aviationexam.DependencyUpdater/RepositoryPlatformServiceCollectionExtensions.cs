using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Interfaces.Repository;
using Aviationexam.DependencyUpdater.Repository.DevOps;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Aviationexam.DependencyUpdater;

public static class RepositoryPlatformServiceCollectionExtensions
{
    public static IServiceCollection AddRepositoryPlatform(
        this IServiceCollection services,
        bool shouldRedactHeaderValue = true
    )
    {
        // Register platform-specific implementations
        services.AddRepositoryDevOps(shouldRedactHeaderValue);
        // Future: services.AddRepositoryGitHub(shouldRedactHeaderValue);

        // Register IRepositoryClient factory
        services.AddScoped<IRepositoryClient>(serviceProvider =>
        {
            var platform = serviceProvider.GetRequiredService<EPlatformSelection>();

            return platform switch
            {
                EPlatformSelection.AzureDevOps =>
                    serviceProvider.GetRequiredService<RepositoryAzureDevOpsClient>(),
                EPlatformSelection.GitHub =>
                    throw new NotImplementedException("GitHub support not yet implemented"),
                _ => throw new InvalidOperationException($"Unsupported platform: {platform}")
            };
        });

        // Register Optional<IPackageFeedClient> factory
        services.AddScoped(serviceProvider =>
        {
            var platform = serviceProvider.GetRequiredService<EPlatformSelection>();

            return platform switch
            {
                EPlatformSelection.AzureDevOps =>
                    new Optional<IPackageFeedClient>(
                        serviceProvider.GetRequiredService<AzureArtifactsPackageFeedClient>()
                    ),
                EPlatformSelection.GitHub =>
                    new Optional<IPackageFeedClient>(null),
                _ => new Optional<IPackageFeedClient>(null)
            };
        });

        return services;
    }
}
