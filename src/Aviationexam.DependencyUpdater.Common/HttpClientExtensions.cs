using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Aviationexam.DependencyUpdater.Common;

public static class HttpClientExtensions
{
    public static async Task<HttpResponseMessage> SendAsync(
        this HttpClient httpClient,
        HttpRequestMessage httpRequestMessage,
        TimeSpan timeout,
        CancellationToken cancellationToken
    )
    {
        using var cts = new CancellationTokenSource(timeout);
        using var mergedCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token);

        return await httpClient.SendAsync(httpRequestMessage, mergedCancellationToken.Token);
    }
}
