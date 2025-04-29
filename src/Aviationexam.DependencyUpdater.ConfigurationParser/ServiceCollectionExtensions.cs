using Microsoft.Extensions.DependencyInjection;

namespace Aviationexam.DependencyUpdater.ConfigurationParser;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConfigurationParser(
        this IServiceCollection services
    ) => services
        .AddScoped<ConfigurationFinder>()
        .AddScoped<DependabotConfigurationParser>()
        .AddScoped<DependabotConfigurationLoader>();
}
