using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Interfaces;
using Aviationexam.DependencyUpdater.Nuget.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Aviationexam.DependencyUpdater.Nuget.Services;

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
        RepositoryConfig repositoryConfig
    ) => GetDirectoryPackagesPropsFiles(repositoryConfig)
        .Concat(GetAllCsprojFiles(repositoryConfig))
        .Concat(GetNugetConfig(repositoryConfig))
        .Concat(GetDotnetTools(repositoryConfig));

    public IEnumerable<NugetFile> GetDirectoryPackagesPropsFiles(
        RepositoryConfig repositoryConfig
    )
    {
        foreach (
            var file in filesystem.EnumerateFiles(repositoryConfig.TargetDirectoryPath, "Directory.Packages.props", EnumerateFilesOptions))
        {
            yield return new NugetFile(Path.GetRelativePath(repositoryConfig.RepositoryPath, file), ENugetFileType.DirectoryPackagesProps);
        }
    }

    public IEnumerable<NugetFile> GetAllCsprojFiles(
        RepositoryConfig repositoryConfig
    )
    {
        foreach (var file in filesystem.EnumerateFiles(repositoryConfig.TargetDirectoryPath, "*.csproj", EnumerateFilesOptions))
        {
            yield return new NugetFile(Path.GetRelativePath(repositoryConfig.RepositoryPath, file), ENugetFileType.Csproj);
        }
    }

    public IEnumerable<NugetFile> GetNugetConfig(
        RepositoryConfig repositoryConfig
    )
    {
        foreach (var file in filesystem.EnumerateFiles(repositoryConfig.TargetDirectoryPath, "Nuget.Config", EnumerateFilesOptions))
        {
            var fileName = Path.GetFileName(file);

            if (string.Equals(fileName, "nuget.config", StringComparison.OrdinalIgnoreCase))
            {
                yield return new NugetFile(Path.GetRelativePath(repositoryConfig.RepositoryPath, file), ENugetFileType.NugetConfig);
            }
        }
    }
    public IEnumerable<NugetFile> GetDotnetTools(
        RepositoryConfig repositoryConfig
    )
    {
        foreach (var file in filesystem.EnumerateFiles(repositoryConfig.TargetDirectoryPath, "dotnet-tools.json", EnumerateFilesOptions))
        {
            var fileName = Path.GetFileName(file);

            if (string.Equals(fileName, "dotnet-tools.json", StringComparison.OrdinalIgnoreCase))
            {
                yield return new NugetFile(Path.GetRelativePath(repositoryConfig.RepositoryPath, file), ENugetFileType.DotnetTools);
            }
        }
    }
}
