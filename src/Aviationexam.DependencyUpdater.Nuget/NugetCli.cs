using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Aviationexam.DependencyUpdater.Nuget;

public class NugetCli(
    ILogger<NugetCli> logger
)
{
    public async Task<bool> Restore(
        string workingDirectory,
        CancellationToken cancellationToken
    )
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = "restore --force-evaluate",
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = new Process();
        process.StartInfo = processStartInfo;
        process.EnableRaisingEvents = true;

        var tcs = new TaskCompletionSource<bool>();

        process.Exited += [SuppressMessage("ReSharper", "AccessToDisposedClosure")] (_, _) => tcs.TrySetResult(process.ExitCode == 0);

        process.Start();

        // Optional: read logs to console/log
        _ = Task.Run([SuppressMessage("ReSharper", "AccessToDisposedClosure")] async () =>
        {
            while (!process.StandardOutput.EndOfStream)
            {
                var line = await process.StandardOutput.ReadLineAsync(cancellationToken);
                if (!string.IsNullOrWhiteSpace(line))
                {
                    logger.LogInformation("[restore] {Line}", line);
                }
            }
        }, cancellationToken);

        _ = Task.Run([SuppressMessage("ReSharper", "AccessToDisposedClosure")] async () =>
        {
            while (!process.StandardError.EndOfStream)
            {
                var line = await process.StandardError.ReadLineAsync(cancellationToken);
                if (!string.IsNullOrWhiteSpace(line))
                {
                    logger.LogError("[restore] {Line}", line);
                }
            }
        }, cancellationToken);

        await using var register = cancellationToken.Register([SuppressMessage("ReSharper", "AccessToDisposedClosure")] () => process.Kill(entireProcessTree: true));

        return await tcs.Task;
    }
}
