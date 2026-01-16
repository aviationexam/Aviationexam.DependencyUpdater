using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Repository.DevOps.Dtos;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Aviationexam.DependencyUpdater.Repository.DevOps;

public class AzureDevOpsUndocumentedClient(
    AzureDevOpsConfiguration azureDevOpsConfiguration,
    AzureDevOpsUndocumentedConfiguration azureDevOpsUndocumentedConfiguration,
    Optional<AzCliSideCarConfiguration> azCliSideCarConfiguration,
    HttpClient httpClient,
    TimeProvider timeProvider,
    ILogger<AzureDevOpsUndocumentedClient> logger
)
{
    private readonly TokenCredential _credential = new DefaultAzureCredential();
    private readonly ConcurrentDictionary<string, AzureDevOpsToken> _tokenCache = new();

    private async Task<string?> GetAccessTokenAsync(
        string azureDevopsResourceId,
        CancellationToken cancellationToken
    )
    {
        if (
            _tokenCache.TryGetValue(azureDevopsResourceId, out var cached)
            && cached.ExpiresOn > timeProvider.GetUtcNow().AddMinutes(5)
        )
        {
            return cached.Token;
        }

        var accessToken = azCliSideCarConfiguration.Value is null
            ? await GetAccessTokenLocalAsync(
                azureDevopsResourceId,
                cancellationToken
            )
            : await GetAccessTokenFromSideCarAsync(
                azCliSideCarConfiguration.Value,
                azureDevopsResourceId,
                cancellationToken
            );

        if (accessToken is null)
        {
            return null;
        }

        _tokenCache[azureDevopsResourceId] = accessToken;

        if (logger.IsEnabled(LogLevel.Trace))
        {
            logger.LogTrace("Created AccessToken for {ResourceId}", azureDevopsResourceId);
        }

        return accessToken.Token;
    }

    private async Task<AzureDevOpsToken?> GetAccessTokenFromSideCarAsync(
        AzCliSideCarConfiguration azCliSideCar,
        string azureDevopsResourceId,
        CancellationToken cancellationToken
    )
    {
        var jsonBody = JsonSerializer.Serialize(
            new AzSideCarRequest
            {
                ResourceId = azureDevopsResourceId,
            },
            AzureArtifactsJsonContext.Default.AzSideCarRequest
        );

        var requestUri = new Uri(azCliSideCar.Address, UriKind.Absolute);

        using var azSideCarRequestContent = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        using var azSideCarRequest = new HttpRequestMessage(HttpMethod.Post, requestUri);
        azSideCarRequest.Content = azSideCarRequestContent;
        azSideCarRequest.Headers.Add("Accept", "application/json");
        azSideCarRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            azCliSideCar.Token
        );

        try
        {
            using var azSideCarResponse = await httpClient.SendAsync(
                azSideCarRequest,
                timeout: TimeSpan.FromSeconds(5),
                cancellationToken
            );

            if (azSideCarResponse.StatusCode is not HttpStatusCode.OK)
            {
                if (logger.IsEnabled(LogLevel.Error))
                {
                    logger.LogError(
                        "AZ side car request for {ResourceId} failed with status code {StatusCode} ({StatusCodeNumber}), and response:\n{Response}",
                        azureDevopsResourceId,
                        azSideCarResponse.StatusCode,
                        (int) azSideCarResponse.StatusCode,
                        await azSideCarResponse.Content.ReadAsStringAsync(cancellationToken)
                    );
                }

                return null;
            }

            await using var hierarchyResponseStream = await azSideCarResponse.Content.ReadAsStreamAsync(cancellationToken);
            var response = await JsonSerializer.DeserializeAsync<AzSideCarResponse>(
                hierarchyResponseStream,
                AzureArtifactsJsonContext.Default.AzSideCarResponse,
                cancellationToken
            );

            if (response is null)
            {
                if (logger.IsEnabled(LogLevel.Error))
                {
                    logger.LogError("Failed to deserialize AZ side car response for {ResourceId}.", azureDevopsResourceId);
                }

                return null;
            }

            return new AzureDevOpsToken(response.AccessToken, response.ExpiresOn);
        }
        catch (HttpRequestException e)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                logger.LogError(e, "Failed to connect to the AZ side car: {RequestUri}.", requestUri);
            }

            return null;
        }
    }

    private async Task<AzureDevOpsToken?> GetAccessTokenLocalAsync(
        string azureDevopsResourceId,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var accessToken = await _credential.GetTokenAsync(
                new TokenRequestContext([$"{azureDevopsResourceId}/.default"]),
                cancellationToken
            );

            return new AzureDevOpsToken(accessToken.Token, accessToken.ExpiresOn);
        }
        catch (CredentialUnavailableException e)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                logger.LogError(e, "Failed to obtain AccessToken for {ResourceId}", azureDevopsResourceId);
            }
            return null;
        }
    }

    internal async Task<IReadOnlyCollection<VersionEntry>?> GetContributionHierarchyQueryAsync(
        string packageName,
        CancellationToken cancellationToken
    )
    {
        var accessToken = await GetAccessTokenAsync(
            azureDevOpsUndocumentedConfiguration.AccessTokenResourceId,
            cancellationToken
        );

        const string dataProvider = "ms.azure-artifacts.upstream-versions-data-provider";

        var request = new HierarchyQueryRequest
        {
            ContributionIds = [dataProvider],
            DataProviderContext = new DataProviderContext
            {
                Properties = new Properties
                {
                    ProjectId = azureDevOpsUndocumentedConfiguration.NugetFeedProject,
                    FeedId = azureDevOpsUndocumentedConfiguration.NugetFeedId,
                    Protocol = "NuGet",
                    PackageName = packageName,
                    SourcePage = new SourcePage
                    {
                        Url = $"https://dev.azure.com/{azureDevOpsConfiguration.Organization}/{azureDevOpsUndocumentedConfiguration.NugetFeedProject}/_artifacts/feed/nuget-feed/NuGet/{packageName}/upstreams",
                        RouteId = "ms.azure-artifacts.artifacts-route",
                        RouteValues = new RouteValues
                        {
                            Project = packageName,
                            Wildcard = $"feed/nuget-feed/NuGet/{packageName}/upstreams",
                            Controller = "ContributedPage",
                            Action = "Execute",
                            ServiceHost = azureDevOpsUndocumentedConfiguration.NugetServiceHost,
                        },
                    },
                },
            },
        };
        var jsonBody = JsonSerializer.Serialize(
            request,
            AzureArtifactsJsonContext.Default.HierarchyQueryRequest
        );

        var requestUri = new Uri($"https://pkgs.dev.azure.com/{azureDevOpsConfiguration.Organization}/_apis/Contribution/HierarchyQuery/project/{azureDevOpsUndocumentedConfiguration.NugetFeedProject}", UriKind.Absolute);

        using var hierarchyRequestContent = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        using var hierarchyRequest = new HttpRequestMessage(HttpMethod.Post, requestUri);
        hierarchyRequest.Content = hierarchyRequestContent;
        hierarchyRequest.Headers.Add("Accept", "application/json;api-version=5.0-preview.1;excludeUrls=true;enumsAsNumbers=true;msDateFormat=true;noArrayWrap=true");
        hierarchyRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            accessToken
        );

        using var hierarchyResponse = await httpClient.SendAsync(
            hierarchyRequest,
            timeout: TimeSpan.FromSeconds(15),
            cancellationToken
        );

        if (hierarchyResponse.StatusCode is not HttpStatusCode.OK)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                logger.LogError(
                    "HierarchyQuery failed with status code {StatusCode} ({StatusCodeNumber}), and response:\n{Response}",
                    hierarchyResponse.StatusCode,
                    (int) hierarchyResponse.StatusCode,
                    await hierarchyResponse.Content.ReadAsStringAsync(cancellationToken)
                );
            }

            return null;
        }

        await using var hierarchyResponseStream = await hierarchyResponse.Content.ReadAsStreamAsync(cancellationToken);
        var response = await JsonSerializer.DeserializeAsync<HierarchyQueryResponse>(
            hierarchyResponseStream,
            AzureArtifactsJsonContext.Default.HierarchyQueryResponse,
            cancellationToken
        );

        return response?.DataProviders[dataProvider].Versions;
    }

    public async Task ManualUpstreamIngestionAsync(
        string packageName,
        string packageVersion,
        CancellationToken cancellationToken
    )
    {
        var accessToken = await GetAccessTokenAsync(
            azureDevOpsUndocumentedConfiguration.AccessTokenResourceId,
            cancellationToken
        );

        var requestUri = new Uri(
            $"https://pkgs.dev.azure.com/{azureDevOpsConfiguration.Organization}/{azureDevOpsUndocumentedConfiguration.NugetFeedProject}/_apis/packaging/feeds/{azureDevOpsUndocumentedConfiguration.NugetFeedId}/NuGet/packages/{packageName}/versions/{packageVersion}/ManualUpstreamIngestion",
            UriKind.Absolute
        );

        var jsonBody = JsonSerializer.Serialize(
            new ManualUpstreamIngestionRequest
            {
                IngestFromUpstream = true,
            },
            AzureArtifactsJsonContext.Default.ManualUpstreamIngestionRequest
        );

        using var manualUpstreamIngestionRequestContent = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        using var manualUpstreamIngestionRequest = new HttpRequestMessage(HttpMethod.Post, requestUri);
        manualUpstreamIngestionRequest.Content = manualUpstreamIngestionRequestContent;
        manualUpstreamIngestionRequest.Headers.Add("Accept", "application/json;api-version=7.1-preview.1;excludeUrls=true;enumsAsNumbers=true;msDateFormat=true;noArrayWrap=true");
        manualUpstreamIngestionRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            accessToken
        );
        manualUpstreamIngestionRequest.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/134.0.0.0 Safari/537.36");

        using var manualUpstreamIngestionResponse = await httpClient.SendAsync(
            manualUpstreamIngestionRequest,
            timeout: TimeSpan.FromSeconds(15),
            cancellationToken
        );

        if (manualUpstreamIngestionResponse.StatusCode is not HttpStatusCode.NoContent)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                logger.LogError(
                    "ManualUpstreamIngestion failed with status code {StatusCode}, and response:\n{Response}",
                    manualUpstreamIngestionResponse.StatusCode,
                    await manualUpstreamIngestionResponse.Content.ReadAsStringAsync(cancellationToken)
                );
            }
        }
    }
}
