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

                enable-beta-ecosystems: true

                registries:
                  nuget-feed:
                    type: nuget-feed
                    url: https://pkgs.dev.azure.com/org/orgId/_packaging/nuget-feed/nuget/v3/index.json
                    token: PAT:${{ DEVOPS_TOKEN }}

                updates:
                  - package-ecosystem: "nuget"
                    directory: "/"
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
                    directory: "/"
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

        Assert.Equal(true, response.Value.EnableBetaEcosystems.GetBoolean());

        var nugetUpdate = Assert.Single(response.Value.Updates, x => x.PackageEcosystem == new DependabotConfiguration.Update.PackageEcosystemEntity("nuget"));
        var githubActionsUpdate = Assert.Single(response.Value.Updates, x => x.PackageEcosystem == new DependabotConfiguration.Update.PackageEcosystemEntity("github-actions"));
        Assert.Equal(new DependabotConfiguration.Update.DirectoryEntity("/"), nugetUpdate.DirectoryValue);
        Assert.Equal(new DependabotConfiguration.Update.DirectoryEntity("/"), githubActionsUpdate.DirectoryValue);
        var groups = nugetUpdate.Groups;
        var microsoftGroup = Assert.Single(groups, x => x.Key == "microsoft");
        var xunitGroup = Assert.Single(groups, x => x.Key == "xunit");

        Assert.Equal(["Microsoft.*", "System.*"], microsoftGroup.Value.Patterns);
        Assert.Equal(["xunit", "xunit.*"], xunitGroup.Value.Patterns);

        var nugetRegistry = Assert.Contains("nuget-feed", response.Value.Registries).AsObject;
        Assert.Equal(3, nugetRegistry.Count);
        Assert.True(nugetRegistry.TryGetProperty("type", out var registryType));
        Assert.True(nugetRegistry.TryGetProperty("url", out var registryUrl));
        Assert.True(nugetRegistry.TryGetProperty("token", out var registryToken));
        Assert.Equal("nuget-feed", registryType);
        Assert.Equal("https://pkgs.dev.azure.com/org/orgId/_packaging/nuget-feed/nuget/v3/index.json", registryUrl);
        Assert.Equal("PAT:${{ DEVOPS_TOKEN }}", registryToken);
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
