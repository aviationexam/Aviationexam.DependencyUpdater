using System;
using System.IO;
using System.Linq;

namespace Aviationexam.DependencyUpdater.Nuget.Tests;

public sealed class TemporaryDirectoryProvider : IDisposable
{
    public string TemporaryDirectory { get; }

    public TemporaryDirectoryProvider()
    {
        TemporaryDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(TemporaryDirectory);
    }

    public string GetPath(
        params string[] subPaths
    ) => Path.Combine(new[] { TemporaryDirectory }.Concat(subPaths).ToArray());

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(TemporaryDirectory))
                Directory.Delete(TemporaryDirectory, recursive: true);
        }
        catch
        {
            // Best-effort cleanup. Log if needed.
        }
    }
}
