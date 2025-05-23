using Aviationexam.DependencyUpdater.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Polly;
using Polly.Retry;
using System;
using System.Net.Http;

namespace Aviationexam.DependencyUpdater.Repository.DevOps;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRepositoryDevOps(
        this IServiceCollection services
    ) => services
        .AddHttpClient<AzureDevOpsUndocumentedClient>()
        .AddHttpMessageHandler(static x => new LoggingHandler(x.GetRequiredService<ILogger<AzureDevOpsUndocumentedClient>>()))
        .Services
        .AddScoped<VssHttpMessageHandler>(static x => x.CreateVssHttpMessageHandler())
        .AddScoped<VssConnection>(static x =>
        {
            var devOpsConfiguration = x.GetRequiredService<DevOpsConfiguration>();
            var vssHttpMessageHandler = x.GetRequiredService<VssHttpMessageHandler>();

            return new VssConnection(
                devOpsConfiguration.OrganizationEndpoint,
                vssHttpMessageHandler,
                []
            );
        })
        .AddResiliencePipeline<string, GitPullRequest>(
            $"{nameof(GitHttpClient.CreatePullRequestAsync)}-pipeline",
            static (builder, context) => builder.AddRetry(new RetryStrategyOptions<GitPullRequest>
            {
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromSeconds(3),
                ShouldHandle = new PredicateBuilder<GitPullRequest>().Handle<VssServiceException>(),
                OnRetry = args =>
                {
                    var logger = context.ServiceProvider.GetRequiredService<ILogger<GitHttpClient>>();

                    logger.LogWarning(
                        args.Outcome.Exception,
                        "Error creating pull request (Attempt: {RetryCount}). Retrying in {RetryTimeSpan}...",
                        args.AttemptNumber,
                        args.RetryDelay
                    );

                    return default;
                },
            }).AddTimeout(TimeSpan.FromSeconds(10))
        )
        .AddScoped<IRepositoryClient, RepositoryAzureDevOpsClient>();

    private static VssHttpMessageHandler CreateVssHttpMessageHandler(this IServiceProvider serviceProvider)
    {
        var devOpsConfiguration = serviceProvider.GetRequiredService<DevOpsConfiguration>();
        var logger = serviceProvider.GetRequiredService<ILogger<VssHttpMessageHandler>>();

        return new VssHttpMessageHandler(
            new VssCredentials(new VssBasicCredential(string.Empty, devOpsConfiguration.PersonalAccessToken)),
            new VssClientHttpRequestSettings(),
#pragma warning disable CA2000
            new LoggingHandler(logger, new HttpClientHandler())
#pragma warning restore CA2000
        );
    }
}
