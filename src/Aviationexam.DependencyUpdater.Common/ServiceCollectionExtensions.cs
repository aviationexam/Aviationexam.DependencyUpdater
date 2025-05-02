using Microsoft.Extensions.DependencyInjection;

namespace Aviationexam.DependencyUpdater.Common;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCommon(
        this IServiceCollection services
    ) => services
        .AddScoped<FutureVersionResolver>()
        .AddScoped<IgnoreResolverFactory>();
}
