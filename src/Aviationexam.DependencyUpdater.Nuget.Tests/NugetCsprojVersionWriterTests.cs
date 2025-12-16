using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Interfaces;
using Aviationexam.DependencyUpdater.Nuget.Models;
using Aviationexam.DependencyUpdater.Nuget.Services;
using Aviationexam.DependencyUpdater.Nuget.Writers;
using Aviationexam.DependencyUpdater.TestsInfrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NuGet.Protocol;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Aviationexam.DependencyUpdater.Nuget.Tests;

public class NugetCsprojVersionWriterTests
{
    [Fact]
    public async Task UpdateConditionalItemGroupWorks()
    {
        using var temporaryDirectoryProvider = new TemporaryDirectoryProvider(
            NullLoggerFactory.Instance.CreateLogger(nameof(TemporaryDirectoryProvider))
        );

        var fileSystem = Substitute.For<IFileSystem>();

        var csprojContent =
            // language=csproj
            """
                <Project Sdk="Microsoft.NET.Sdk">

                  <PropertyGroup>
                    <TargetFrameworks>net8.0;net9.0</TargetFrameworks>
                  </PropertyGroup>

                  <ItemGroup Condition="'$(TargetFramework)' == 'net9.0'">
                    <PackageReference Include="Microsoft.Extensions.Hosting" Version="9.0.0" />
                  </ItemGroup>

                  <ItemGroup Condition="'$(TargetFramework)' == 'net8.0'">
                    <PackageReference Include="Microsoft.Extensions.Hosting" Version="8.0.0" />
                  </ItemGroup>

                </Project>
                """;

        await using var csprojStream = csprojContent.AsStream();

        var filePath = temporaryDirectoryProvider.GetPath("Project.csproj");

        fileSystem
            .FileOpen(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None)
            .Returns(csprojStream);

        var writer = new NugetCsprojVersionWriter(fileSystem);

        var nugetUpdateCandidate = new NugetUpdateCandidate(
            new NugetDependency(
                new NugetFile("Project.csproj", ENugetFileType.Csproj),
                new NugetPackageReference("Microsoft.Extensions.Hosting", new VersionRange(new NuGetVersion("9.0.0"))),
                [new NugetTargetFramework("net9.0")]
            ),
            new PossiblePackageVersion(
                new PackageVersion<PackageSearchMetadataRegistration>(
                    new PackageVersion(new Version("9.0.1"), false, [], NugetReleaseLabelComparer.Instance),
                    new Dictionary<EPackageSource, PackageSearchMetadataRegistration>()
                ),
                []
            )
        );

        var result = await writer.TrySetVersionAsync(
            nugetUpdateCandidate,
            filePath,
            new Dictionary<string, PackageVersion>(),
            TestContext.Current.CancellationToken
        );

        Assert.Equal(ESetVersion.VersionSet, result);

        csprojStream.Position = 0;
        using var reader = new StreamReader(csprojStream);
        var updatedContent = await reader.ReadToEndAsync(TestContext.Current.CancellationToken);

        Assert.Contains("Version=\"9.0.1\"", updatedContent);
        Assert.Contains("Condition=\"'$(TargetFramework)' == 'net9.0'\"", updatedContent);
        // Ensure the net8.0 version was not changed
        Assert.Contains("Version=\"8.0.0\"", updatedContent);
    }

    [Fact]
    public async Task UpdateConditionalPackageReferenceElementWorks()
    {
        using var temporaryDirectoryProvider = new TemporaryDirectoryProvider(
            NullLoggerFactory.Instance.CreateLogger(nameof(TemporaryDirectoryProvider))
        );

        var fileSystem = Substitute.For<IFileSystem>();

        var csprojContent =
            // language=csproj
            """
                <Project Sdk="Microsoft.NET.Sdk">

                  <PropertyGroup>
                    <TargetFrameworks>net8.0;net9.0</TargetFrameworks>
                  </PropertyGroup>

                  <ItemGroup>
                    <PackageReference Include="Microsoft.Extensions.Hosting" Version="9.0.0" Condition="'$(TargetFramework)' == 'net9.0'" />
                    <PackageReference Include="Microsoft.Extensions.Hosting" Version="8.0.0" Condition="'$(TargetFramework)' == 'net8.0'" />
                  </ItemGroup>

                </Project>
                """;

        await using var csprojStream = csprojContent.AsStream();

        var filePath = temporaryDirectoryProvider.GetPath("Project.csproj");

        fileSystem
            .FileOpen(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None)
            .Returns(csprojStream);

        var writer = new NugetCsprojVersionWriter(fileSystem);

        var nugetUpdateCandidate = new NugetUpdateCandidate(
            new NugetDependency(
                new NugetFile("Project.csproj", ENugetFileType.Csproj),
                new NugetPackageReference("Microsoft.Extensions.Hosting", new VersionRange(new NuGetVersion("8.0.0"))),
                [new NugetTargetFramework("net8.0")]
            ),
            new PossiblePackageVersion(
                new PackageVersion<PackageSearchMetadataRegistration>(
                    new PackageVersion(new Version("8.0.10"), false, [], NugetReleaseLabelComparer.Instance),
                    new Dictionary<EPackageSource, PackageSearchMetadataRegistration>()
                ),
                []
            )
        );

        var result = await writer.TrySetVersionAsync(
            nugetUpdateCandidate,
            filePath,
            new Dictionary<string, PackageVersion>(),
            TestContext.Current.CancellationToken
        );

        Assert.Equal(ESetVersion.VersionSet, result);

        csprojStream.Position = 0;
        using var reader = new StreamReader(csprojStream);
        var updatedContent = await reader.ReadToEndAsync(TestContext.Current.CancellationToken);

        Assert.Contains("Version=\"8.0.10\"", updatedContent);
        Assert.Contains("Condition=\"'$(TargetFramework)' == 'net8.0'\"", updatedContent);
        // Ensure the net9.0 version was not changed
        Assert.Contains("Version=\"9.0.0\"", updatedContent);
    }

    [Fact]
    public async Task UpdateUnconditionalPackageReferenceWorks()
    {
        using var temporaryDirectoryProvider = new TemporaryDirectoryProvider(
            NullLoggerFactory.Instance.CreateLogger(nameof(TemporaryDirectoryProvider))
        );

        var fileSystem = Substitute.For<IFileSystem>();

        var csprojContent =
            // language=csproj
            """
                <Project Sdk="Microsoft.NET.Sdk">

                  <PropertyGroup>
                    <TargetFrameworks>net8.0;net9.0</TargetFrameworks>
                  </PropertyGroup>

                  <ItemGroup>
                    <PackageReference Include="Meziantou.Analyzer" Version="2.0.0" />
                  </ItemGroup>

                </Project>
                """;

        await using var csprojStream = csprojContent.AsStream();

        var filePath = temporaryDirectoryProvider.GetPath("Project.csproj");

        fileSystem
            .FileOpen(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None)
            .Returns(csprojStream);

        var writer = new NugetCsprojVersionWriter(fileSystem);

        var nugetUpdateCandidate = new NugetUpdateCandidate(
            new NugetDependency(
                new NugetFile("Project.csproj", ENugetFileType.Csproj),
                new NugetPackageReference("Meziantou.Analyzer", new VersionRange(new NuGetVersion("2.0.0"))),
                [
                    new NugetTargetFramework("net8.0"),
                    new NugetTargetFramework("net9.0"),
                ]
            ),
            new PossiblePackageVersion(
                new PackageVersion<PackageSearchMetadataRegistration>(
                    new PackageVersion(new Version("2.0.177"), false, [], NugetReleaseLabelComparer.Instance),
                    new Dictionary<EPackageSource, PackageSearchMetadataRegistration>()
                ),
                []
            )
        );

        var result = await writer.TrySetVersionAsync(
            nugetUpdateCandidate,
            filePath,
            new Dictionary<string, PackageVersion>(),
            TestContext.Current.CancellationToken
        );

        Assert.Equal(ESetVersion.VersionSet, result);

        csprojStream.Position = 0;
        using var reader = new StreamReader(csprojStream);
        var updatedContent = await reader.ReadToEndAsync(TestContext.Current.CancellationToken);

        Assert.Contains("Version=\"2.0.177\"", updatedContent);
        Assert.Contains("Meziantou.Analyzer", updatedContent);
    }
}
