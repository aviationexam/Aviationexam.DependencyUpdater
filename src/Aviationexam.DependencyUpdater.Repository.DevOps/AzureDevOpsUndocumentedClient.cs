using Aviationexam.DependencyUpdater.Repository.DevOps.Dtos;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Aviationexam.DependencyUpdater.Repository.DevOps;

public class AzureDevOpsUndocumentedClient(
    DevOpsConfiguration devOpsConfiguration,
    DevOpsUndocumentedConfiguration devOpsUndocumentedConfiguration,
    HttpClient httpClient,
    TimeProvider timeProvider,
    ILogger<AzureDevOpsUndocumentedClient> logger
)
{
    private readonly TokenCredential _credential = new DefaultAzureCredential();
    private readonly ConcurrentDictionary<string, AzureDevOpsToken> _tokenCache = new();

    private async Task<string> GetAccessTokenAsync(
        string azureDevopsResourceId,
        CancellationToken cancellationToken
    )
    {
        var cacheKey = $"{azureDevopsResourceId}/.default";

        if (
            _tokenCache.TryGetValue(cacheKey, out var cached)
            && cached.ExpiresOn > timeProvider.GetUtcNow().AddMinutes(5)
        )
        {
            return cached.Token;
        }

        var accessToken = await _credential.GetTokenAsync(
            new TokenRequestContext([cacheKey]),
            cancellationToken
        );

        _tokenCache[cacheKey] = new AzureDevOpsToken(accessToken.Token, accessToken.ExpiresOn);

        logger.LogTrace("Created AccessToken for {ResourceId}", azureDevopsResourceId);

        return accessToken.Token;
    }

    internal async Task<IReadOnlyCollection<VersionEntry>?> GetContributionHierarchyQueryAsync(
        string packageName,
        CancellationToken cancellationToken
    )
    {
        var accessToken = await GetAccessTokenAsync(
            devOpsUndocumentedConfiguration.AccessTokenResourceId,
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
                    ProjectId = devOpsConfiguration.Project,
                    FeedId = devOpsUndocumentedConfiguration.NugetFeedId,
                    Protocol = "NuGet",
                    PackageName = packageName,
                    SourcePage = new SourcePage
                    {
                        Url = $"https://dev.azure.com/{devOpsConfiguration.Organization}/{devOpsConfiguration.Project}/_artifacts/feed/nuget-feed/NuGet/{packageName}/upstreams",
                        RouteId = "ms.azure-artifacts.artifacts-route",
                        RouteValues = new RouteValues
                        {
                            Project = packageName,
                            Wildcard = $"feed/nuget-feed/NuGet/{packageName}/upstreams",
                            Controller = "ContributedPage",
                            Action = "Execute",
                            ServiceHost = devOpsUndocumentedConfiguration.NugetServiceHost,
                        },
                    },
                },
            },
        };
        var jsonBody = JsonSerializer.Serialize(
            request,
            AzureArtifactsJsonContext.Default.HierarchyQueryRequest
        );

        var requestUri = new Uri($"https://pkgs.dev.azure.com/{devOpsConfiguration.Organization}/_apis/Contribution/HierarchyQuery/project/{devOpsConfiguration.Project}", UriKind.Absolute);

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
            cancellationToken
        );

        if (!hierarchyResponse.IsSuccessStatusCode)
        {
            logger.LogError(
                "HierarchyQuery failed with status code {StatusCode}, and response:\n{Response}",
                hierarchyResponse.StatusCode,
                await hierarchyResponse.Content.ReadAsStringAsync(cancellationToken)
            );

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
            devOpsUndocumentedConfiguration.AccessTokenResourceId,
            cancellationToken
        );

        var requestUri = new Uri(
            $"https://pkgs.dev.azure.com/{devOpsConfiguration.Organization}/{devOpsConfiguration.Project}/_apis/packaging/feeds/{devOpsUndocumentedConfiguration.NugetFeedId}/NuGet/packages/{packageName}/versions/{packageVersion}/ManualUpstreamIngestion",
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
            cancellationToken
        );

        if (manualUpstreamIngestionResponse.StatusCode is not System.Net.HttpStatusCode.NoContent)
        {
            logger.LogError(
                "ManualUpstreamIngestion failed with status code {StatusCode}, and response:\n{Response}",
                manualUpstreamIngestionResponse.StatusCode,
                await manualUpstreamIngestionResponse.Content.ReadAsStringAsync(cancellationToken)
            );
        }
    }
}
