using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Aviationexam.DependencyUpdater.Common;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCommon(
        this IServiceCollection services, SourceConfiguration sourceConfiguration
    ) => services
        .AddSingleton(Options.Create(sourceConfiguration))
        .AddScoped<FutureVersionResolver>()
        .AddScoped<IgnoreResolverFactory>()
        .AddScoped<GroupResolverFactory>();
}
