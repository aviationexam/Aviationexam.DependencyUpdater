using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Aviationexam.DependencyUpdater.Repository.DevOps;

public class LoggingHandler(
    ILogger logger,
    HttpMessageHandler innerHandler
) : DelegatingHandler(innerHandler)
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        logger.LogTrace("Request: {Method} {RequestUri}", request.Method, request.RequestUri);

        if (request.Content != null)
        {
            await request.Content.LoadIntoBufferAsync(cancellationToken);
            var requestBody = await request.Content.ReadAsStringAsync(cancellationToken);

            logger.LogTrace("Request Body: {RequestBody}", requestBody);
        }

        var response = await base.SendAsync(request, cancellationToken);

        logger.LogTrace("Response: {StatusCode}", response.StatusCode);

        await response.Content.LoadIntoBufferAsync(cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        logger.LogTrace("Response Body: {ResponseBody}", responseBody);

        return response;
    }
}
