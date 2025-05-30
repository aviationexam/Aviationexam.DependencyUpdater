using Aviationexam.DependencyUpdater.Interfaces;
using System.Collections.Generic;
using System.IO;

namespace Aviationexam.DependencyUpdater.DefaultImplementations;

public class FileSystem : IFileSystem
{
    public IEnumerable<string> EnumerateFiles(
        string path, string searchPattern, EnumerationOptions enumerationOptions
    ) => Directory.EnumerateFiles(path, searchPattern, enumerationOptions);

    public bool Exists(string path) => File.Exists(path);

    public Stream FileOpen(
        string path, FileMode mode, FileAccess access, FileShare share
    ) => File.Open(path, mode, access, share);
}
