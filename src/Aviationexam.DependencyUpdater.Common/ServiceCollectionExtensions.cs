using Microsoft.Extensions.DependencyInjection;

namespace Aviationexam.DependencyUpdater.Common;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCommon(
        this IServiceCollection services, SourceConfiguration sourceConfiguration
    ) => services
        .AddSingleton(sourceConfiguration)
        .AddScoped<FutureVersionResolver>()
        .AddScoped<IgnoreResolverFactory>()
        .AddScoped<GroupResolverFactory>();
}
