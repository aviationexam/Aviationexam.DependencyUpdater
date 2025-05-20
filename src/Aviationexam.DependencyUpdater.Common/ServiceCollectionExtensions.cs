using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace Aviationexam.DependencyUpdater.Common;

public static class ServiceCollectionExtensions
{
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Safe due to source-generated binding")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Safe due to source-generated binding")]
    public static IServiceCollection AddCommon(
        this IServiceCollection services, IConfigurationRoot configuration
    ) => services
        .Configure<SourceConfiguration>(configuration.GetSection("Source"))
        .AddScoped<FutureVersionResolver>()
        .AddScoped<IgnoreResolverFactory>()
        .AddScoped<GroupResolverFactory>();
}
