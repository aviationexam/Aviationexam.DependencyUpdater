using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Aviationexam.DependencyUpdater.Repository.DevOps;

public class LoggingHandler : DelegatingHandler
{
    private readonly ILogger _logger;

    public LoggingHandler(
        ILogger logger
    )
    {
        _logger = logger;
    }

    public LoggingHandler(
        ILogger logger,
        HttpMessageHandler innerHandler
    ) : base(innerHandler)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _logger.LogTrace("Request: {Method} {RequestUri}", request.Method, request.RequestUri);

        if (request.Content != null)
        {
            await request.Content.LoadIntoBufferAsync(cancellationToken);
            var requestBody = await request.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogTrace("Request Body: {RequestBody}", requestBody);
        }

        var response = await base.SendAsync(request, cancellationToken);

        _logger.LogTrace("Response: {StatusCode}", response.StatusCode);

        await response.Content.LoadIntoBufferAsync(cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        _logger.LogTrace("Response Body: {ResponseBody}", responseBody);

        return response;
    }
}
