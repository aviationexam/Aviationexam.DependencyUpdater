using Aviationexam.DependencyUpdater.Interfaces;
using Aviationexam.DependencyUpdater.TestsInfrastructure;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.IO;
using Xunit;

namespace Aviationexam.DependencyUpdater.ConfigurationParser.Tests;

public class DependabotConfigurationParserTests
{
    [Fact]
    public void ParseWorks()
    {
        using var temporaryDirectoryProvider = new TemporaryDirectoryProvider();

        var fileSystem = Substitute.For<IFileSystem>();
        var logger = Substitute.For<ILogger<DependabotConfigurationParser>>();

        fileSystem
            .Exists(temporaryDirectoryProvider.GetPath("dependabot.yml"))
            .Returns(true);

        using var dependabotYmlStream =
            // language=yml
            """
                # To get started with Dependabot version updates, you'll need to specify which
                # package ecosystems to update and where the package manifests are located.
                # Please see the documentation for all configuration options:
                # https://help.github.com/github/administering-a-repository/configuration-options-for-dependency-updates

                version: 2
                updates:
                  - package-ecosystem: "nuget"
                    directory: "/" # Location of package manifests
                    schedule:
                      interval: "daily"
                    groups:
                      microsoft:
                        patterns:
                          - Microsoft.*
                          - System.*
                      xunit:
                        patterns:
                          - xunit
                          - xunit.*

                  - package-ecosystem: "github-actions"
                    directory: "/" # Location of package manifests
                    schedule:
                      interval: "daily"

                """.AsStream();

        fileSystem
            .FileOpen(temporaryDirectoryProvider.GetPath("dependabot.yml"), FileMode.Open, FileAccess.Read, FileShare.Read)
            .Returns(dependabotYmlStream);

        var dependabotConfigurationParser = new DependabotConfigurationParser(
            fileSystem,
            logger
        );

        var response = dependabotConfigurationParser.Parse(temporaryDirectoryProvider.GetPath("dependabot.yml"));

        Assert.NotNull(response);
    }


    [Fact]
    public void ParseFails()
    {
        var directoryPath = "/opt/asp.net/repository";

        var fileSystem = Substitute.For<IFileSystem>();
        var logger = Substitute.For<ILogger<DependabotConfigurationParser>>();

        fileSystem
            .Exists($"{directoryPath}/dependabot.yml")
            .Returns(false);

        var directoryPackagesPropsParser = new DependabotConfigurationParser(
            fileSystem,
            logger
        );

        var response = directoryPackagesPropsParser.Parse($"{directoryPath}/dependabot.yml");

        Assert.Null(response);
    }
}
