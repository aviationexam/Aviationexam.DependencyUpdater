using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aviationexam.DependencyUpdater.Common;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCommon(
        this IServiceCollection services, IConfigurationRoot configuration
    ) => services
        .Configure<SourceConfiguration>(configuration.GetSection("Source"))
        .AddScoped<FutureVersionResolver>()
        .AddScoped<IgnoreResolverFactory>()
        .AddScoped<GroupResolverFactory>();
}
