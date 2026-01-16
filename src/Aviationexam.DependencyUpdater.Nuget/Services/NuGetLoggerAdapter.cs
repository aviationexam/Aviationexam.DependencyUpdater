using Microsoft.Extensions.Logging;
using NuGet.Common;
using System.Threading.Tasks;
using ILogger = NuGet.Common.ILogger;
using LogLevel = NuGet.Common.LogLevel;

namespace Aviationexam.DependencyUpdater.Nuget.Services;

public class NuGetLoggerAdapter(
    ILogger<NuGetLoggerAdapter> logger
) : ILogger
{
    public void LogDebug(string data)
    {
        if (logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
        {
            logger.LogDebug(data);
        }
    }

    public void LogVerbose(string data)
    {
        if (logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Trace))
        {
            logger.LogTrace(data);
        }
    }

    public void LogInformation(string data)
    {
        if (logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Information))
        {
            logger.LogInformation(data);
        }
    }

    public void LogMinimal(string data)
    {
        if (logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Information))
        {
            logger.LogInformation(data);
        }
    }

    public void LogWarning(string data)
    {
        if (logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Warning))
        {
            logger.LogWarning(data);
        }
    }

    public void LogError(string data)
    {
        if (logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Error))
        {
            logger.LogError(data);
        }
    }

    public void LogInformationSummary(string data)
    {
        if (logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Information))
        {
            logger.LogInformation(data);
        }
    }

    public void Log(LogLevel level, string data)
    {
        var mappedLevel = level switch
        {
            LogLevel.Debug => Microsoft.Extensions.Logging.LogLevel.Debug,
            LogLevel.Verbose => Microsoft.Extensions.Logging.LogLevel.Trace,
            LogLevel.Information => Microsoft.Extensions.Logging.LogLevel.Information,
            LogLevel.Minimal => Microsoft.Extensions.Logging.LogLevel.Information,
            LogLevel.Warning => Microsoft.Extensions.Logging.LogLevel.Warning,
            LogLevel.Error => Microsoft.Extensions.Logging.LogLevel.Error,
            _ => Microsoft.Extensions.Logging.LogLevel.None
        };

        if (logger.IsEnabled(mappedLevel))
        {
            logger.Log(mappedLevel, data);
        }
    }

    public Task LogAsync(LogLevel level, string data)
    {
        Log(level, data);

        return Task.CompletedTask;
    }

    public void Log(ILogMessage message)
    {
        Log(message.Level, message.Message);
    }

    public Task LogAsync(ILogMessage message)
    {
        Log(message);

        return Task.CompletedTask;
    }
}
