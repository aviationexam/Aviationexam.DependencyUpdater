using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Repository.Abstractions;
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
            var platform = serviceProvider.GetRequiredService<PlatformSelection>();

            return platform switch
            {
                PlatformSelection.AzureDevOps =>
                    serviceProvider.GetRequiredService<RepositoryAzureDevOpsClient>(),
                PlatformSelection.GitHub =>
                    throw new NotImplementedException("GitHub support not yet implemented"),
                _ => throw new InvalidOperationException($"Unsupported platform: {platform}")
            };
        });

        // Register Optional<IPackageFeedClient> factory
        services.AddScoped(serviceProvider =>
        {
            var platform = serviceProvider.GetRequiredService<PlatformSelection>();

            return platform switch
            {
                PlatformSelection.AzureDevOps =>
                    new Optional<IPackageFeedClient>(
                        serviceProvider.GetRequiredService<AzureArtifactsPackageFeedClient>()
                    ),
                PlatformSelection.GitHub =>
                    new Optional<IPackageFeedClient>(null),
                _ => new Optional<IPackageFeedClient>(null)
            };
        });

        return services;
    }
}
