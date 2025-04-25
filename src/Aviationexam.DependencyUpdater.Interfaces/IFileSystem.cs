using System.Collections.Generic;
using System.IO;

namespace Aviationexam.DependencyUpdater.Interfaces;

public interface IFileSystem
{
    IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption);
}
