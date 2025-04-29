using Aviationexam.DependencyUpdater.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Aviationexam.DependencyUpdater.DefaultImplementations;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDefaultImplementations(
        this IServiceCollection services
    )
    {
        services.TryAddScoped<IFileSystem, FileSystem>();
        services.TryAddScoped<IEnvVariableProvider, EnvVariableProvider>();

        return services;
    }
}
