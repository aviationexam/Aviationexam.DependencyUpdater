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
    ) => services
        .AddRepositoryDevOps(shouldRedactHeaderValue)
        // Future: services.AddRepositoryGitHub(shouldRedactHeaderValue);
        .AddScoped<IRepositoryClient>(serviceProvider =>
        {
            var platform = serviceProvider.GetRequiredService<EPlatformSelection>();

            return platform switch
            {
                EPlatformSelection.AzureDevOps =>
                    serviceProvider.GetRequiredService<RepositoryAzureDevOpsClient>(),
                EPlatformSelection.GitHub =>
                    throw new NotImplementedException("GitHub support not yet implemented"),
                _ => throw new ArgumentOutOfRangeException(nameof(platform), platform, null),
            };
        })
        .AddScoped(serviceProvider =>
        {
            var platform = serviceProvider.GetRequiredService<EPlatformSelection>();

            return platform switch
            {
                EPlatformSelection.AzureDevOps => new Optional<IPackageFeedClient>(
                    serviceProvider.GetRequiredService<AzureArtifactsPackageFeedClient>()
                ),
                EPlatformSelection.GitHub => new Optional<IPackageFeedClient>(null),
                _ => throw new ArgumentOutOfRangeException(nameof(platform), platform, null),
            };
        });
}
