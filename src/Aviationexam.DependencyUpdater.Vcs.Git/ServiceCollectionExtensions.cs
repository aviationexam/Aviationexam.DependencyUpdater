using Aviationexam.DependencyUpdater.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Aviationexam.DependencyUpdater.Vcs.Git;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddVcsGit(
        this IServiceCollection services
    )
    {
        services.TryAddScoped<ISourceVersioningFactory, GitSourceVersioningFactory>();

        return services;
    }
}
