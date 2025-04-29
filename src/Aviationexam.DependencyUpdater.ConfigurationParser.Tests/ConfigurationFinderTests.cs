using Aviationexam.DependencyUpdater.Interfaces;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using Xunit.Sdk;

namespace Aviationexam.DependencyUpdater.ConfigurationParser.Tests;

public class ConfigurationFinderTests
{
    [Fact]
    public void GetAllDependabotFilesWorks()
    {
        var directoryPath = "/opt/asp.net/repository";

        var fileSystem = Substitute.For<IFileSystem>();

        fileSystem
            .Exists($"{directoryPath}/.github/updater.yml")
            .Returns(false);

        fileSystem
            .Exists($"{directoryPath}/.azuredevops/updater.yml")
            .Returns(false);

        fileSystem
            .Exists($"{directoryPath}/.github/dependabot.yml")
            .Returns(true);

        fileSystem
            .Exists($"{directoryPath}/.azuredevops/dependabot.yml")
            .Returns(true);

        var configurationFinder = new ConfigurationFinder(fileSystem);

        var response = configurationFinder.GetAllDependabotFiles(directoryPath);

        Assert.Equal([
            $"{directoryPath}/.github/dependabot.yml",
            $"{directoryPath}/.azuredevops/dependabot.yml",
        ], response);
    }

    [Fact]
    public void GetAllDependabotFilesWorks_()
    {
        var directoryPath = "/opt/asp.net/repository";

        var fileSystem = Substitute.For<IFileSystem>();

        fileSystem
            .Exists($"{directoryPath}/.github/updater.yml")
            .Returns(true);

        fileSystem
            .Exists($"{directoryPath}/.azuredevops/updater.yml")
            .Returns(true);

        fileSystem
            .Exists($"{directoryPath}/.github/dependabot.yml")
            .Throws(_ => new XunitException(".github/dependabot.yml should not be searched for"));

        fileSystem
            .Exists($"{directoryPath}/.azuredevops/dependabot.yml")
            .Throws(_ => new XunitException(".azuredevops/dependabot.yml should not be searched for"));

        var configurationFinder = new ConfigurationFinder(fileSystem);

        var response = configurationFinder.GetAllDependabotFiles(directoryPath);

        Assert.Equal([
            $"{directoryPath}/.github/updater.yml",
            $"{directoryPath}/.azuredevops/updater.yml",
        ], response);
    }
}
