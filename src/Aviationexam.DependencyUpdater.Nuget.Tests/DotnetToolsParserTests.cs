using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Interfaces;
using Aviationexam.DependencyUpdater.Nuget.Models;
using Aviationexam.DependencyUpdater.Nuget.Parsers;
using Aviationexam.DependencyUpdater.TestsInfrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using System.IO;
using Xunit;

namespace Aviationexam.DependencyUpdater.Nuget.Tests;

public class DotnetToolsParserTests
{
    [Fact]
    public void ParseWorks()
    {
        using var temporaryDirectoryProvider = new TemporaryDirectoryProvider(
            NullLoggerFactory.Instance.CreateLogger<TemporaryDirectoryProvider>()
        );

        var fileSystem = Substitute.For<IFileSystem>();
        var logger = Substitute.For<ILogger<DotnetToolsParser>>();

        Directory.CreateDirectory(temporaryDirectoryProvider.GetPath("project"));
        fileSystem
            .Exists(temporaryDirectoryProvider.GetPath(".config/dotnet-tools.json"))
            .Returns(true);

        using var projectFileStream =
            // language=json
            """
                {
                  "version": 1,
                  "isRoot": true,
                  "tools": {
                    "dotnet-ef": {
                      "version": "9.0.0",
                      "commands": [
                        "dotnet-ef"
                      ]
                    },
                    "dotnet-svcutil": {
                      "version": "2.2.0-preview1.23462.5",
                      "commands": [
                        "dotnet-svcutil"
                      ]
                    },
                    "dotnet-xscgen": {
                      "version": "2.1.1174",
                      "commands": [
                        "xscgen"
                      ]
                    }
                  }
                }
                """.AsStream();

        fileSystem
            .FileOpen(temporaryDirectoryProvider.GetPath(".config/dotnet-tools.json"), FileMode.Open, FileAccess.Read, FileShare.Read)
            .Returns(projectFileStream);

        var csprojParser = new DotnetToolsParser(
            fileSystem,
            logger
        );

        var nugetFile = new NugetFile(".config/dotnet-tools.json", ENugetFileType.DotnetTools);
        var response = csprojParser.Parse(temporaryDirectoryProvider.TemporaryDirectory, nugetFile);

        Assert.Equal([
            new NugetDependency(nugetFile, new NugetPackageVersion("dotnet-ef", "9.0.0"), []),
            new NugetDependency(nugetFile, new NugetPackageVersion("dotnet-svcutil", "2.2.0-preview1.23462.5"), []),
            new NugetDependency(nugetFile, new NugetPackageVersion("dotnet-xscgen", "2.1.1174"), []),
        ], response);
    }

    [Fact]
    public void ParseFails()
    {
        var directoryPath = "/opt/asp.net/repository";

        var fileSystem = Substitute.For<IFileSystem>();
        var logger = Substitute.For<ILogger<DotnetToolsParser>>();

        fileSystem
            .Exists($"{directoryPath}/.config/dotnet-tools.json")
            .Returns(false);

        var csprojParser = new DotnetToolsParser(
            fileSystem,
            logger
        );

        var response = csprojParser.Parse(directoryPath, new NugetFile(".config/dotnet-tools.json", ENugetFileType.Csproj));

        Assert.Empty(response);
    }
}
