using Aviationexam.DependencyUpdater.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Aviationexam.DependencyUpdater.Nuget;

public sealed class NugetFinder(
    IFileSystem filesystem
)
{
    public IEnumerable<NugetFile> GetAllNugetFiles(
        string directoryPath
    ) => GetDirectoryPackagesPropsFiles(directoryPath)
        .Concat(GetAllCsprojFiles(directoryPath));

    public IEnumerable<NugetFile> GetDirectoryPackagesPropsFiles(
        string directoryPath
    )
    {
        foreach (var file in filesystem.EnumerateFiles(directoryPath, "Directory.Packages.props", SearchOption.AllDirectories))
        {
            yield return new NugetFile(file, ENugetFileType.DirectoryPackagesProps);
        }
    }

    public IEnumerable<NugetFile> GetAllCsprojFiles(
        string directoryPath
    )
    {
        foreach (var file in filesystem.EnumerateFiles(directoryPath, "*.csproj", SearchOption.AllDirectories))
        {
            yield return new NugetFile(file, ENugetFileType.Csproj);
        }
    }
}
