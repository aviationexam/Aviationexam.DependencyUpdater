using Aviationexam.DependencyUpdater.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;

namespace Aviationexam.DependencyUpdater.Repository.DevOps;

public static class ServiceCollectionExtensions
{
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Safe due to source-generated binding")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Safe due to source-generated binding")]
    public static IServiceCollection AddRepositoryDevOps(
        this IServiceCollection services, IConfigurationRoot configuration
    ) => services
        .Configure<DevOpsConfiguration>(configuration.GetSection("DevOps"))
        .Configure<DevOpsUndocumentedConfiguration>(configuration.GetSection("DevOps"))
        .AddHttpClient<AzureDevOpsUndocumentedClient>()
        .AddHttpMessageHandler(x => new LoggingHandler(x.GetRequiredService<ILogger<AzureDevOpsUndocumentedClient>>()))
        .Services
        .AddScoped<VssHttpMessageHandler>(x => x.CreateVssHttpMessageHandler())
        .AddScoped<VssConnection>(x =>
        {
            var devOpsConfiguration = x.GetRequiredService<IOptionsSnapshot<DevOpsConfiguration>>();
            var vssHttpMessageHandler = x.GetRequiredService<VssHttpMessageHandler>();

            return new VssConnection(
                devOpsConfiguration.Value.OrganizationEndpoint,
                vssHttpMessageHandler,
                []
            );
        })
        .AddScoped<IRepositoryClient, RepositoryAzureDevOpsClient>();

    private static VssHttpMessageHandler CreateVssHttpMessageHandler(this IServiceProvider serviceProvider)
    {
        var devOpsConfiguration = serviceProvider.GetRequiredService<IOptionsSnapshot<DevOpsConfiguration>>();
        var logger = serviceProvider.GetRequiredService<ILogger<VssHttpMessageHandler>>();

        return new VssHttpMessageHandler(
            new VssCredentials(new VssBasicCredential(string.Empty, devOpsConfiguration.Value.PersonalAccessToken)),
            new VssClientHttpRequestSettings(),
#pragma warning disable CA2000
            new LoggingHandler(logger, new HttpClientHandler())
#pragma warning restore CA2000
        );
    }
}
