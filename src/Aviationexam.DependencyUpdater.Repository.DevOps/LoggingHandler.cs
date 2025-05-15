using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Aviationexam.DependencyUpdater.Repository.DevOps;

public class LoggingHandler(
    ILogger logger
) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        logger.LogTrace("Request: {Method} {RequestUri}", request.Method, request.RequestUri);

        var response = await base.SendAsync(request, cancellationToken);

        logger.LogTrace("Response: {StatusCode}", response.StatusCode);
        return response;
    }
}
