using Aviationexam.DependencyUpdater.Interfaces;
using NSubstitute;
using System.IO;
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

        var response = nugetFinder.GetAllNugetFiles(directoryPath);

        Assert.Equal([
            new NugetFile($"{directoryPath}/Directory.Packages.props", ENugetFileType.DirectoryPackagesProps),
            new NugetFile($"{directoryPath}/project/Project.csproj", ENugetFileType.Csproj),
            new NugetFile($"{directoryPath}/project2/Project2.csproj", ENugetFileType.Csproj),
            new NugetFile($"{directoryPath}/nuget.config", ENugetFileType.NugetConfig),
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

        var response = nugetFinder.GetDirectoryPackagesPropsFiles(directoryPath);

        Assert.Equal([
            new NugetFile($"{directoryPath}/Directory.Packages.props", ENugetFileType.DirectoryPackagesProps),
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

        var response = nugetFinder.GetAllCsprojFiles(directoryPath);

        Assert.Equal([
            new NugetFile($"{directoryPath}/project/Project.csproj", ENugetFileType.Csproj),
            new NugetFile($"{directoryPath}/project2/Project2.csproj", ENugetFileType.Csproj),
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

        var response = nugetFinder.GetNugetConfig(directoryPath);

        Assert.Equal([
            new NugetFile($"{directoryPath}/nuget.config", ENugetFileType.NugetConfig),
        ], response);
    }
}
