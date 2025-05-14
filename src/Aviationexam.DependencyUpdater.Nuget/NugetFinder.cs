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
    internal static EnumerationOptions EnumerateFilesOptions { get; } = new()
    {
        RecurseSubdirectories = true,
        MatchType = MatchType.Simple,
        AttributesToSkip = FileAttributes.None,
        IgnoreInaccessible = false,
        MatchCasing = MatchCasing.CaseInsensitive,
    };


    public IEnumerable<NugetFile> GetAllNugetFiles(
        string repositoryPath,
        string directoryPath
    ) => GetDirectoryPackagesPropsFiles(repositoryPath, directoryPath)
        .Concat(GetAllCsprojFiles(repositoryPath, directoryPath))
        .Concat(GetNugetConfig(repositoryPath, directoryPath));

    public IEnumerable<NugetFile> GetDirectoryPackagesPropsFiles(
        string repositoryPath,
        string directoryPath
    )
    {
        foreach (
            var file in filesystem.EnumerateFiles(directoryPath, "Directory.Packages.props", EnumerateFilesOptions))
        {
            yield return new NugetFile(Path.GetRelativePath(repositoryPath, file), ENugetFileType.DirectoryPackagesProps);
        }
    }

    public IEnumerable<NugetFile> GetAllCsprojFiles(
        string repositoryPath,
        string directoryPath
    )
    {
        foreach (var file in filesystem.EnumerateFiles(directoryPath, "*.csproj", EnumerateFilesOptions))
        {
            yield return new NugetFile(Path.GetRelativePath(repositoryPath, file), ENugetFileType.Csproj);
        }
    }

    public IEnumerable<NugetFile> GetNugetConfig(
        string repositoryPath,
        string directoryPath
    )
    {
        foreach (var file in filesystem.EnumerateFiles(directoryPath, "Nuget.Config", EnumerateFilesOptions))
        {
            var fileName = Path.GetFileName(file);

            if (string.Equals(fileName, "nuget.config", StringComparison.OrdinalIgnoreCase))
            {
                yield return new NugetFile(Path.GetRelativePath(repositoryPath, file), ENugetFileType.NugetConfig);
            }
        }
    }
}
