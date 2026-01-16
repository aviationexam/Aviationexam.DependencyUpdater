using Microsoft.Extensions.Logging;
using System;
using System.IO;
using ZLinq;

namespace Aviationexam.DependencyUpdater.Common;

public sealed class TemporaryDirectoryProvider : IDisposable
{
    private readonly ILogger _logger;

    public string TemporaryDirectory { get; }

    public TemporaryDirectoryProvider(
        ILogger logger,
        bool create = true
    )
    {
        _logger = logger;
        TemporaryDirectory = Path.Join(Path.GetTempPath(), Path.GetRandomFileName());
        if (create)
        {
            Directory.CreateDirectory(TemporaryDirectory);
        }
    }

    public string GetPath(
        params string[] subPaths
    ) => Path.Join(new[] { TemporaryDirectory }.AsValueEnumerable().Concat(subPaths).ToArray());

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(TemporaryDirectory))
            {
                if (_logger.IsEnabled(LogLevel.Trace))
                {
                    _logger.LogTrace("Deleting temporary directory: {TemporaryDirectory}", TemporaryDirectory);
                }

                Directory.Delete(TemporaryDirectory, recursive: true);
            }
            else
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Unable to delete temporary directory: {TemporaryDirectory}. It does not exists anymore.", TemporaryDirectory);
                }
            }
        }
        catch (Exception e)
        {
            if (_logger.IsEnabled(LogLevel.Error))
            {
                _logger.LogError(e, "Unable to delete temporary directory: {TemporaryDirectory}", TemporaryDirectory);
            }
        }
    }
}
