using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Interfaces;
using Aviationexam.DependencyUpdater.TestsInfrastructure;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.IO;
using System.Linq;
using Xunit;

namespace Aviationexam.DependencyUpdater.Nuget.Tests;

public class NugetConfigParserTests
{
    [Fact]
    public void ParseWorks()
    {
        using var temporaryDirectoryProvider = new TemporaryDirectoryProvider();

        var fileSystem = Substitute.For<IFileSystem>();
        var logger = Substitute.For<ILogger<NugetConfigParser>>();

        fileSystem
            .Exists(temporaryDirectoryProvider.GetPath("nuget.config"))
            .Returns(true);

        using var projectFileStream =
            // language=xml
            """
                <?xml version="1.0" encoding="utf-8"?>

                <configuration>
                    <packageSources>
                        <clear />
                        <add key="nuget-feed"
                             value="https://pkgs.dev.azure.com/org/orgId/_packaging/nuget-feed/nuget/v3/index.json"
                             protocolVersion="3" />
                        <add key="reporting-api"
                             value="https://f.feedz.io/aviationexam/reporting-api/nuget/index.json"
                             protocolVersion="3" />
                    </packageSources>
                    <packageSourceMapping>
                        <packageSource key="reporting-api">
                            <package pattern="ReportingApi" />
                        </packageSource>

                        <packageSource key="nuget-feed">
                            <package pattern="*" />
                        </packageSource>
                    </packageSourceMapping>
                </configuration>
                """.AsStream();

        fileSystem
            .FileOpen(temporaryDirectoryProvider.GetPath("nuget.config"), FileMode.Open, FileAccess.Read, FileShare.Read)
            .Returns(projectFileStream);

        var nugetConfigParser = new NugetConfigParser(
            fileSystem,
            logger
        );

        var nugetConfigFile = new NugetFile(temporaryDirectoryProvider.GetPath("nuget.config"), ENugetFileType.NugetConfig);
        var response = nugetConfigParser.Parse(nugetConfigFile);

        Assert.Equal([
            new NugetSource("nuget-feed", "https://pkgs.dev.azure.com/org/orgId/_packaging/nuget-feed/nuget/v3/index.json", NugetSourceVersion.V3, PackageMapping:
            [
                new NugetPackageSourceMap("*"),
            ]),
            new NugetSource("reporting-api", "https://f.feedz.io/aviationexam/reporting-api/nuget/index.json", NugetSourceVersion.V3, PackageMapping:
            [
                new NugetPackageSourceMap("ReportingApi"),
            ]),
        ], response);
    }

    [Fact]
    public void ParseFails()
    {
        var directoryPath = "/opt/asp.net/repository";

        var fileSystem = Substitute.For<IFileSystem>();
        var logger = Substitute.For<ILogger<NugetConfigParser>>();

        fileSystem
            .Exists($"{directoryPath}/nuget.config")
            .Returns(false);

        var nugetConfigParser = new NugetConfigParser(
            fileSystem,
            logger
        );

        var response = nugetConfigParser.Parse(new NugetFile($"{directoryPath}/nuget.config", ENugetFileType.NugetConfig));

        Assert.Empty(response);
    }
}
