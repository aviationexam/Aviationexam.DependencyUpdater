using Aviationexam.DependencyUpdater.Repository.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Polly;
using Polly.Retry;
using System;
using System.Net.Http;
using System.Threading;

namespace Aviationexam.DependencyUpdater.Repository.DevOps;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRepositoryDevOps(
        this IServiceCollection serviceCollection,
        bool shouldRedactHeaderValue = true
    )
    {
        var httpClientBuilder = serviceCollection
            .AddHttpClient<AzureDevOpsUndocumentedClient>(x => x.Timeout = Timeout.InfiniteTimeSpan)
            .AddDefaultLogger()
            .AddHttpMessageHandler(static x => new LoggingHandler(x.GetRequiredService<ILogger<AzureDevOpsUndocumentedClient>>()));

        if (shouldRedactHeaderValue is false)
        {
            serviceCollection
                .Configure<HttpClientFactoryOptions>(httpClientBuilder.Name, x => x.ShouldRedactHeaderValue = _ => false);
        }

        return serviceCollection
            .AddScoped<VssHttpMessageHandler>(static x => x.CreateVssHttpMessageHandler())
            .AddScoped<VssConnection>(static x =>
            {
                var devOpsConfiguration = x.GetRequiredService<AzureDevOpsConfiguration>();
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
            .AddScoped<RepositoryAzureDevOpsClient>()
            .AddScoped<AzureArtifactsPackageFeedClient>();
    }

    private static VssHttpMessageHandler CreateVssHttpMessageHandler(this IServiceProvider serviceProvider)
    {
        var devOpsConfiguration = serviceProvider.GetRequiredService<AzureDevOpsConfiguration>();
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
