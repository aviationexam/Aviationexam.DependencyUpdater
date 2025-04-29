using System.Collections.Generic;
using System.IO;

namespace Aviationexam.DependencyUpdater.Interfaces;

public interface IFileSystem
{
    IEnumerable<string> EnumerateFiles(string path, string searchPattern, EnumerationOptions enumerationOptions);

    bool Exists(string path);

    Stream FileOpen(
        string path, FileMode mode, FileAccess access, FileShare share
    );
}
