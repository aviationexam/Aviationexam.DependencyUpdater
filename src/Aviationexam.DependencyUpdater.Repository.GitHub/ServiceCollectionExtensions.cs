using Aviationexam.DependencyUpdater.Interfaces;
using Aviationexam.DependencyUpdater.Interfaces.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Octokit;
using Polly;
using Polly.Retry;
using System;

namespace Aviationexam.DependencyUpdater.Repository.GitHub;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRepositoryGitHub(
        this IServiceCollection serviceCollection,
        bool shouldRedactHeaderValue = true
    )
    {
        var httpClientBuilder = serviceCollection
            .AddHttpClient("GitHub")
            .AddDefaultLogger();

        if (shouldRedactHeaderValue is false)
        {
            serviceCollection
                .Configure<HttpClientFactoryOptions>(httpClientBuilder.Name, x => x.ShouldRedactHeaderValue = _ => false);
        }

        return serviceCollection
            .AddScoped<IGitHubClient>(static serviceProvider =>
            {
                var gitHubConfiguration = serviceProvider.GetRequiredService<GitHubConfiguration>();

                var client = new GitHubClient(new ProductHeaderValue("Aviationexam.DependencyUpdater"))
                {
                    Credentials = new Credentials(gitHubConfiguration.Token),
                };

                return client;
            })
            .AddResiliencePipeline<string, Octokit.PullRequest>(
                $"{nameof(IGitHubClient.PullRequest.Create)}-pipeline",
                static (builder, context) => builder.AddRetry(new RetryStrategyOptions<Octokit.PullRequest>
                {
                    MaxRetryAttempts = 3,
                    BackoffType = DelayBackoffType.Exponential,
                    Delay = TimeSpan.FromSeconds(3),
                    ShouldHandle = new PredicateBuilder<Octokit.PullRequest>()
                        .Handle<ApiException>()
                        .Handle<RateLimitExceededException>(),
                    OnRetry = args =>
                    {
                        var logger = context.ServiceProvider.GetRequiredService<ILogger<IGitHubClient>>();

                        if (logger.IsEnabled(LogLevel.Warning))
                        {
                            logger.LogWarning(
                                args.Outcome.Exception,
                                "Error creating pull request (Attempt: {RetryCount}). Retrying in {RetryTimeSpan}...",
                                args.AttemptNumber,
                                args.RetryDelay
                            );
                        }

                        return default;
                    },
                }).AddTimeout(TimeSpan.FromSeconds(10))
            )
            .AddKeyedScoped<IRepositoryClient, RepositoryGitHubClient>(EPlatformSelection.GitHub);
    }
}
