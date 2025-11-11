using Aviationexam.DependencyUpdater.Nuget.Configurations;
using Aviationexam.DependencyUpdater.Nuget.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ZLinq;
using NugetJsonContext = Aviationexam.DependencyUpdater.Nuget.Models.NugetJsonContext;

namespace Aviationexam.DependencyUpdater.Nuget.Services;

public class NugetCli(
    ILogger<NugetCli> logger
)
{
    public async Task<bool> Restore(
        string workingDirectory,
        NugetAuthConfig authConfig,
        CancellationToken cancellationToken
    )
    {
        var vssNugetExternalFeedEndpoints = new VssNugetExternalFeedEndpoints
        {
            EndpointCredentials =
            [
                .. authConfig.NugetFeedAuthentications.AsValueEnumerable().Select(x => new VssNugetExternalFeedEndpointCredential
                {
                    Endpoint = x.FeedUrl,
                    Username = x.Username!,
                    Password = x.Password!,
                }),
            ],
        };
        var vssNugetExternalFeedEndpointsJson = JsonSerializer.Serialize(vssNugetExternalFeedEndpoints, NugetJsonContext.Default.VssNugetExternalFeedEndpoints);

        var processStartInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = "restore --force-evaluate",
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            Environment =
            {
                ["VSS_NUGET_EXTERNAL_FEED_ENDPOINTS"] = vssNugetExternalFeedEndpointsJson,
            },
        };

        using var process = new Process();
        process.StartInfo = processStartInfo;
        process.EnableRaisingEvents = true;

        var tcs = new TaskCompletionSource<bool>();

        process.Exited += [SuppressMessage("ReSharper", "AccessToDisposedClosure")] (_, _) =>
        {
            logger.Log(
                process.ExitCode is 0 ? LogLevel.Trace : LogLevel.Error,
                "The dotnet restore exit code is: {ExitCode}",
                process.ExitCode
            );

            tcs.TrySetResult(process.ExitCode == 0);
        };

        process.Start();

        // Optional: read logs to console/log
        _ = Task.Run([SuppressMessage("ReSharper", "AccessToDisposedClosure")] async () =>
        {
            while (await process.StandardOutput.ReadLineAsync(cancellationToken) is {} line)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    logger.LogTrace("[restore] {Line}", line);
                }
            }
        }, cancellationToken);

        _ = Task.Run([SuppressMessage("ReSharper", "AccessToDisposedClosure")] async () =>
        {
            while (await process.StandardOutput.ReadLineAsync(cancellationToken) is {} line)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    logger.LogError("[restore] {Line}", line);
                }
            }
        }, cancellationToken);

        await using var register = cancellationToken.Register([SuppressMessage("ReSharper", "AccessToDisposedClosure")] () =>
        {
            logger.LogError("The dotnet restore killed");

            process.Kill(entireProcessTree: true);
        });

        return await tcs.Task;
    }
}
