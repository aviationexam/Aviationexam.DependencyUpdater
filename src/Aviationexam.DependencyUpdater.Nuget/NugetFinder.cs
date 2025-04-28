using Aviationexam.DependencyUpdater.Interfaces;
using System;
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
        .Concat(GetAllCsprojFiles(directoryPath))
        .Concat(GetNugetConfig(directoryPath));

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

    public IEnumerable<NugetFile> GetNugetConfig(
        string directoryPath
    )
    {
        foreach (var file in filesystem.EnumerateFiles(directoryPath, "*.config", SearchOption.AllDirectories))
        {
            var fileName = Path.GetFileName(file);

            if (string.Equals(fileName, "nuget.config", StringComparison.OrdinalIgnoreCase))
            {
                yield return new NugetFile(file, ENugetFileType.NugetConfig);
            }
        }
    }
}
