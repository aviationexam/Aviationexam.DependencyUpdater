using Aviationexam.DependencyUpdater.Interfaces;
using Aviationexam.DependencyUpdater.Interfaces.ReleaseNotes;
using Aviationexam.DependencyUpdater.Interfaces.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Octokit;
using Octokit.Internal;
using Polly;
using Polly.Retry;
using System;

namespace Aviationexam.DependencyUpdater.Repository.GitHub;

public static class ServiceCollectionExtensions
{
    public const string AuthenticationProxy = "AuthenticationProxy";
    public const string ProjectReleases = "ProjectReleases";

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
            .AddScoped<ICredentialStore>(static serviceProvider => new InMemoryCredentialStore(
                new Credentials(serviceProvider.GetRequiredService<GitHubConfiguration>().Token)
            ))
            .AddScoped<IGitHubClient>(static serviceProvider => new GitHubClient(
                new ProductHeaderValue("Aviationexam.DependencyUpdater"),
                serviceProvider.GetRequiredService<ICredentialStore>()
            ))
            .AddKeyedScoped<IGitHubClient>(AuthenticationProxy, static (
                    serviceProvider, _
                ) => serviceProvider.GetRequiredService<GitHubConfiguration>().AuthenticationProxyAddress is { } authenticationProxyAddress
                    ? new GitHubClient(
                        new ProductHeaderValue("Aviationexam.DependencyUpdater"),
                        serviceProvider.GetRequiredService<ICredentialStore>(),
                        baseAddress: authenticationProxyAddress
                    )
                    : serviceProvider.GetRequiredService<IGitHubClient>()
            )
            // Release notes of third-party packages are read from public GitHub repositories;
            // the platform token is used when the tool runs against GitHub, otherwise anonymous access applies.
            .AddKeyedScoped<IGitHubClient>(ProjectReleases, static (
                    serviceProvider, _
                ) => serviceProvider.GetService<GitHubConfiguration>() is not null
                    ? serviceProvider.GetRequiredService<IGitHubClient>()
                    : new GitHubClient(new ProductHeaderValue("Aviationexam.DependencyUpdater"))
            )
            .AddScoped<IProjectReleasesProvider, GitHubProjectReleasesProvider>()
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
