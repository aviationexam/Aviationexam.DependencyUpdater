namespace Aviationexam.DependencyUpdater.Common;

public sealed class TemporaryDirectoryProvider : IDisposable
{
    public string TemporaryDirectory { get; }

    public TemporaryDirectoryProvider(bool create = true)
    {
        TemporaryDirectory = Path.Join(Path.GetTempPath(), Path.GetRandomFileName());
        if (create)
        {
            Directory.CreateDirectory(TemporaryDirectory);
        }
    }

    public string GetPath(
        params string[] subPaths
    ) => Path.Join(new[] { TemporaryDirectory }.Concat(subPaths).ToArray());

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(TemporaryDirectory))
            {
                Directory.Delete(TemporaryDirectory, recursive: true);
            }
        }
        catch
        {
            // Best-effort cleanup. Log if needed.
        }
    }
}
