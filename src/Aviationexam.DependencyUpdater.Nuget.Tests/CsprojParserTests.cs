using Aviationexam.DependencyUpdater.Interfaces;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.IO;
using Xunit;

namespace Aviationexam.DependencyUpdater.Nuget.Tests;

public class CsprojParserTests
{
    [Fact]
    public void ParseWorks()
    {
        var directoryPath = "/opt/asp.net/repository";

        var fileSystem = Substitute.For<IFileSystem>();
        var logger = Substitute.For<ILogger<CsprojParser>>();

        fileSystem
            .Exists($"{directoryPath}/project/Project.csproj")
            .Returns(true);

        fileSystem
            .Exists($"{directoryPath}/WarningConfiguration.targets")
            .Returns(true);

        using var projectFileStream =
            // language=csproj
            """
                <Project Sdk="Microsoft.NET.Sdk">

                  <PropertyGroup>
                    <TargetFramework>net9.0</TargetFramework>
                  </PropertyGroup>

                  <Import Project="..\WarningConfiguration.targets" />

                  <ItemGroup>
                    <PackageReference Include="Microsoft.Extensions.Hosting" />
                    <PackageReference Include="Microsoft.Extensions.Logging.Console" />
                  </ItemGroup>

                </Project>
                """.AsStream();

        fileSystem
            .FileOpen($"{directoryPath}/project/Project.csproj", FileMode.Open, FileAccess.Read, FileShare.Read)
            .Returns(projectFileStream);

        using var warningConfigurationStream =
            // language=targets
            """
                <Project>

                  <ItemGroup>
                    <PackageReference Include="Meziantou.Analyzer">
                      <PrivateAssets>all</PrivateAssets>
                      <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
                    </PackageReference>
                  </ItemGroup>

                </Project>
                """.AsStream();

        fileSystem
            .FileOpen($"{directoryPath}/WarningConfiguration.targets", FileMode.Open, FileAccess.Read, FileShare.Read)
            .Returns(warningConfigurationStream);

        var csprojParser = new CsprojParser(
            fileSystem,
            logger
        );

        var nugetFile = new NugetFile($"{directoryPath}/project/Project.csproj", ENugetFileType.Csproj);
        var warningConfigurationFile = new NugetFile($"{directoryPath}/WarningConfiguration.targets", ENugetFileType.Targets);
        var response = csprojParser.Parse(nugetFile);

        Assert.Equal([
            new NugetDependency(nugetFile, new NugetPackageReference("Microsoft.Extensions.Hosting", null)),
            new NugetDependency(nugetFile, new NugetPackageReference("Microsoft.Extensions.Logging.Console", null)),
            new NugetDependency(warningConfigurationFile, new NugetPackageReference("Meziantou.Analyzer", null)),
        ], response);
    }

    public void ParseFails()
    {
        var directoryPath = "/opt/asp.net/repository";

        var fileSystem = Substitute.For<IFileSystem>();
        var logger = Substitute.For<ILogger<CsprojParser>>();

        fileSystem
            .Exists($"{directoryPath}/project/Project.csproj")
            .Returns(false);

        var csprojParser = new CsprojParser(
            fileSystem,
            logger
        );

        var response = csprojParser.Parse(new NugetFile($"{directoryPath}/project/Project.csproj", ENugetFileType.Csproj));

        Assert.Empty(response);
    }
}
