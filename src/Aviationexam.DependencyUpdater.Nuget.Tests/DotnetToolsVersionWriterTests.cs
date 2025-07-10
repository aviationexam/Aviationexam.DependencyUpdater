using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Interfaces;
using Aviationexam.DependencyUpdater.Nuget.Models;
using Aviationexam.DependencyUpdater.Nuget.Services;
using Aviationexam.DependencyUpdater.Nuget.Tests.Infrastructure;
using Aviationexam.DependencyUpdater.Nuget.Writers;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NuGet.Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Aviationexam.DependencyUpdater.Nuget.Tests;

public class DotnetToolsVersionWriterTests
{
    [Fact]
    public async Task TrySetVersionWorks_ValidJson_UpdatesVersion()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var logger = Substitute.For<ILogger<DotnetToolsVersionWriter>>();

        await using var fileStream = new MemoryStream(
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
                    }
                  }
                }
                """u8.ToArray()
        );

        await using var proxyFileStream = new StreamProxy(fileStream);

        fileSystem
            .FileOpen(
                ".config/dotnet-tools.json",
                FileMode.Open,
                FileAccess.ReadWrite,
                FileShare.None
            )
            .Returns(proxyFileStream);

        var versionWriter = new DotnetToolsVersionWriter(fileSystem, logger);
        var updateCandidate = new NugetUpdateCandidate(
            new NugetDependency(
                new NugetFile(".config/dotnet-tools.json", ENugetFileType.DotnetTools),
                new NugetPackageVersion("dotnet-ef", "9.0.0"),
                [new NugetTargetFramework("net9.0")]
            ),
            new PossiblePackageVersion(new PackageVersion<PackageSearchMetadataRegistration>(
                new PackageVersion(new Version("10.0.0"), false, [], NugetReleaseLabelComparer.Instance),
                new Dictionary<EPackageSource, PackageSearchMetadataRegistration>()
            ), [])
        );

        // Act
        var result = await versionWriter.TrySetVersionAsync(
            updateCandidate,
            ".config/dotnet-tools.json",
            new Dictionary<string, PackageVersion>(),
            TestContext.Current.CancellationToken
        );

        Assert.True(proxyFileStream.WasDisposed);

        // Assert
        Assert.Equal(ESetVersion.VersionSet, result);
        fileStream.Position = 0;
        using var reader = new StreamReader(fileStream);

        Assert.Equal(
            // language=json
            """
            {
              "version": 1,
              "isRoot": true,
              "tools": {
                "dotnet-ef": {
                  "version": "10.0.0",
                  "commands": [
                    "dotnet-ef"
                  ]
                }
              }
            }
            """.ReplaceLineEndings("\n"),
            await reader.ReadToEndAsync(TestContext.Current.CancellationToken)
        );
    }

    [Fact]
    public async Task TrySetVersionFails_PackageNotFound_ReturnsVersionNotSet()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var logger = Substitute.For<ILogger<DotnetToolsVersionWriter>>();

        await using var fileStream = new MemoryStream(
            // language=json
            """
            {
              "version": 1,
              "isRoot": true,
              "tools": {
                "other-tool": {
                  "version": "1.0.0",
                  "commands": [
                    "other-tool"
                  ]
                }
              }
            }
            """u8.ToArray()
        );

        await using var proxyFileStream = new StreamProxy(fileStream);

        fileSystem
            .FileOpen(
                ".config/dotnet-tools.json",
                FileMode.Open,
                FileAccess.ReadWrite,
                FileShare.None
            )
            .Returns(proxyFileStream);

        var versionWriter = new DotnetToolsVersionWriter(fileSystem, logger);
        var updateCandidate = new NugetUpdateCandidate(
            new NugetDependency(
                new NugetFile(".config/dotnet-tools.json", ENugetFileType.DotnetTools),
                new NugetPackageVersion("dotnet-ef", "9.0.0"),
                [new NugetTargetFramework("net9.0")]
            ),
            new PossiblePackageVersion(new PackageVersion<PackageSearchMetadataRegistration>(
                new PackageVersion(new Version("10.0.0"), false, [], NugetReleaseLabelComparer.Instance),
                new Dictionary<EPackageSource, PackageSearchMetadataRegistration>()
            ), [])
        );

        // Act
        var result = await versionWriter.TrySetVersionAsync(
            updateCandidate,
            ".config/dotnet-tools.json",
            new Dictionary<string, PackageVersion>(),
            TestContext.Current.CancellationToken
        );

        Assert.True(proxyFileStream.WasDisposed);
        Assert.Equal(ESetVersion.VersionNotSet, result);
    }

    [Fact]
    public async Task TrySetVersionFails_InvalidJson_ReturnsVersionNotSet()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var logger = Substitute.For<ILogger<DotnetToolsVersionWriter>>();

        await using var fileStream = new MemoryStream(
            // language=json
            "invalid json"u8.ToArray()
        );

        await using var proxyFileStream = new StreamProxy(fileStream);

        fileSystem
            .FileOpen(
                ".config/dotnet-tools.json",
                FileMode.Open,
                FileAccess.ReadWrite,
                FileShare.None
            )
            .Returns(proxyFileStream);

        var versionWriter = new DotnetToolsVersionWriter(fileSystem, logger);
        var updateCandidate = new NugetUpdateCandidate(
            new NugetDependency(
                new NugetFile(".config/dotnet-tools.json", ENugetFileType.DotnetTools),
                new NugetPackageVersion("dotnet-ef", "9.0.0"),
                [new NugetTargetFramework("net9.0")]
            ),
            new PossiblePackageVersion(new PackageVersion<PackageSearchMetadataRegistration>(
                new PackageVersion(new Version("10.0.0"), false, [], NugetReleaseLabelComparer.Instance),
                new Dictionary<EPackageSource, PackageSearchMetadataRegistration>()
            ), [])
        );

        // Act
        var result = await versionWriter.TrySetVersionAsync(
            updateCandidate,
            ".config/dotnet-tools.json",
            new Dictionary<string, PackageVersion>(),
            TestContext.Current.CancellationToken
        );

        Assert.True(proxyFileStream.WasDisposed);
        Assert.Equal(ESetVersion.VersionNotSet, result);
    }
}
