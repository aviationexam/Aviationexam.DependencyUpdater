using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Aviationexam.DependencyUpdater.Nuget;

public sealed class NugetFinder(
    string directoryPath
)
{
    public IEnumerable<NugetFile> GetAllNugetFiles() => GetDirectoryPackagesPropsFiles()
        .Concat(GetAllCsprojFiles());

    public IEnumerable<NugetFile> GetDirectoryPackagesPropsFiles()
    {
        foreach (var file in Directory.EnumerateFiles(directoryPath, "Directory.Packages.props", SearchOption.AllDirectories))
        {
            yield return new NugetFile(file, ENugetFileType.DirectoryPackagesProps);
        }
    }

    public IEnumerable<NugetFile> GetAllCsprojFiles()
    {
        foreach (var file in Directory.EnumerateFiles(directoryPath, "*.csproj", SearchOption.AllDirectories))
        {
            yield return new NugetFile(file, ENugetFileType.Csproj);
        }
    }
}
