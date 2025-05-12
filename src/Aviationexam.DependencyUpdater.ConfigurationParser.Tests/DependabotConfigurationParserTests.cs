using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Interfaces;
using Aviationexam.DependencyUpdater.TestsInfrastructure;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.IO;
using System.Linq;
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
                  nuget.org:
                    type: nuget-feed
                    url: https://api.nuget.org/v3/index.json
                    nuget-feed-version: V3

                updates:
                  - package-ecosystem: "nuget"
                    directory: "/"
                    targetFramework: net9.0
                    registries:
                      - nuget-feed
                    fallback-registries:
                      nuget-feed: nuget.org
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
        Assert.Equal(new TargetFrameworkEntity("net9.0"), nugetUpdate.TargetFramework);
        Assert.Equal(new DependabotConfiguration.Update.DirectoryEntity("/"), githubActionsUpdate.DirectoryValue);
        var groups = nugetUpdate.Groups;
        var microsoftGroup = Assert.Single(groups, x => x.Key == "microsoft");
        var xunitGroup = Assert.Single(groups, x => x.Key == "xunit");

        Assert.Equal(["Microsoft.*", "System.*"], microsoftGroup.Value.Patterns);
        Assert.Equal(["xunit", "xunit.*"], xunitGroup.Value.Patterns);

        var nugetRegistry = Assert.Contains("nuget-feed", response.Value.Registries).As<DependabotConfiguration.Registry.Entity>();
        var nugetOrgRegistry = Assert.Contains("nuget.org", response.Value.Registries).As<DependabotConfiguration.Registry.Entity>();
        Assert.Equal(3, nugetRegistry.Count);
        Assert.Equal(new DependabotConfiguration.Registry.Entity.TypeEntity("nuget-feed"), nugetRegistry.Type);
        Assert.Equal("https://pkgs.dev.azure.com/org/orgId/_packaging/nuget-feed/nuget/v3/index.json", nugetRegistry.Url);
        Assert.Equal("PAT:${{ DEVOPS_TOKEN }}", nugetRegistry.Token);

        Assert.Equal(3, nugetOrgRegistry.Count);
        Assert.Equal(new DependabotConfiguration.Registry.Entity.TypeEntity("nuget-feed"), nugetOrgRegistry.Type);
        Assert.Equal("https://api.nuget.org/v3/index.json", nugetOrgRegistry.Url);
        Assert.Equal("V3", nugetOrgRegistry.NugetFeedVersion.GetString());

        var registry = nugetUpdate.Registries.Single();
        var fallbackRegistry = nugetUpdate.FallbackRegistries.Single();

        Assert.Equal("nuget-feed", registry.AsString.GetString());
        Assert.Equal("nuget-feed", fallbackRegistry.Key);
        Assert.Equal("nuget.org", fallbackRegistry.Value);
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
