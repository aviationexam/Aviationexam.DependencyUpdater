using Aviationexam.DependencyUpdater.Interfaces;
using System.Collections.Generic;
using System.IO;

namespace Aviationexam.DependencyUpdater.DefaultImplementations;

public class FileSystem : IFileSystem
{
    public IEnumerable<string> EnumerateFiles(
        string path, string searchPattern, SearchOption searchOption
    ) => Directory.EnumerateFiles(path, searchPattern, searchOption);
}
