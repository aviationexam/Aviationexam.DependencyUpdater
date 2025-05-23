using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Interfaces;
using NSubstitute;
using Xunit;

namespace Aviationexam.DependencyUpdater.Nuget.Tests;

public class NugetFinderTests
{
    [Fact]
    public void GetAllNugetFilesWorks()
    {
        var directoryPath = "/opt/asp.net/repository";

        var fileSystem = Substitute.For<IFileSystem>();
        fileSystem
            .EnumerateFiles(directoryPath, "Directory.Packages.props", NugetFinder.EnumerateFilesOptions)
            .Returns([$"{directoryPath}/Directory.Packages.props"]);

        fileSystem
            .EnumerateFiles(directoryPath, "*.csproj", NugetFinder.EnumerateFilesOptions)
            .Returns([$"{directoryPath}/project/Project.csproj", $"{directoryPath}/project2/Project2.csproj"]);

        fileSystem
            .EnumerateFiles(directoryPath, "Nuget.Config", NugetFinder.EnumerateFilesOptions)
            .Returns([$"{directoryPath}/nuget.config"]);

        var nugetFinder = new NugetFinder(fileSystem);

        var response = nugetFinder.GetAllNugetFiles(new RepositoryConfig
        {
            RepositoryPath = directoryPath,
            SubdirectoryPath = directoryPath,
        });

        Assert.Equal([
            new NugetFile("Directory.Packages.props", ENugetFileType.DirectoryPackagesProps),
            new NugetFile("project/Project.csproj", ENugetFileType.Csproj),
            new NugetFile("project2/Project2.csproj", ENugetFileType.Csproj),
            new NugetFile("nuget.config", ENugetFileType.NugetConfig),
        ], response);
    }

    [Fact]
    public void GetDirectoryPackagesPropsFilesWorks()
    {
        var directoryPath = "/opt/asp.net/repository";

        var fileSystem = Substitute.For<IFileSystem>();
        fileSystem
            .EnumerateFiles(directoryPath, "Directory.Packages.props", NugetFinder.EnumerateFilesOptions)
            .Returns([$"{directoryPath}/Directory.Packages.props"]);

        var nugetFinder = new NugetFinder(fileSystem);

        var response = nugetFinder.GetDirectoryPackagesPropsFiles(new RepositoryConfig
        {
            RepositoryPath = directoryPath,
            SubdirectoryPath = directoryPath,
        });

        Assert.Equal([
            new NugetFile("Directory.Packages.props", ENugetFileType.DirectoryPackagesProps),
        ], response);
    }

    [Fact]
    public void GetAllCsprojFilesWorks()
    {
        var directoryPath = "/opt/asp.net/repository";

        var fileSystem = Substitute.For<IFileSystem>();

        fileSystem
            .EnumerateFiles(directoryPath, "*.csproj", NugetFinder.EnumerateFilesOptions)
            .Returns([$"{directoryPath}/project/Project.csproj", $"{directoryPath}/project2/Project2.csproj"]);

        var nugetFinder = new NugetFinder(fileSystem);

        var response = nugetFinder.GetAllCsprojFiles(new RepositoryConfig
        {
            RepositoryPath = directoryPath,
            SubdirectoryPath = directoryPath,
        });

        Assert.Equal([
            new NugetFile("project/Project.csproj", ENugetFileType.Csproj),
            new NugetFile("project2/Project2.csproj", ENugetFileType.Csproj),
        ], response);
    }

    [Fact]
    public void GetNugetConfigWorks()
    {
        var directoryPath = "/opt/asp.net/repository";

        var fileSystem = Substitute.For<IFileSystem>();
        fileSystem
            .EnumerateFiles(directoryPath, "Nuget.Config", NugetFinder.EnumerateFilesOptions)
            .Returns([$"{directoryPath}/nuget.config"]);

        var nugetFinder = new NugetFinder(fileSystem);

        var response = nugetFinder.GetNugetConfig(new RepositoryConfig
        {
            RepositoryPath = directoryPath,
            SubdirectoryPath = directoryPath,
        });

        Assert.Equal([
            new NugetFile("nuget.config", ENugetFileType.NugetConfig),
        ], response);
    }
}
