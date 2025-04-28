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
            .EnumerateFiles(directoryPath, "Directory.Packages.props", SearchOption.AllDirectories)
            .Returns([$"{directoryPath}/Directory.Packages.props"]);

        fileSystem
            .EnumerateFiles(directoryPath, "*.csproj", SearchOption.AllDirectories)
            .Returns([$"{directoryPath}/project/Project.csproj", $"{directoryPath}/project2/Project2.csproj"]);

        fileSystem
            .EnumerateFiles(directoryPath, "*.config", SearchOption.AllDirectories)
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
}
