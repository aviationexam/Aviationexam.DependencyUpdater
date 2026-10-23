using Aviationexam.DependencyUpdater.Interfaces;
using NSubstitute;
using System;
using Xunit;

namespace Aviationexam.DependencyUpdater.Common.Tests;

public class EnvironmentVariablesExtensionsTests
{
    [Fact]
    public void PopulateEnvironmentVariablesWorks()
    {
        var envVariableProvider = Substitute.For<IEnvVariableProvider>();
        envVariableProvider.GetEnvironmentVariable("WORLD")
            .Returns("World");

        var response = envVariableProvider.PopulateEnvironmentVariables("Hello ${{WORLD}}");

        Assert.Equal("Hello World", response);
    }

    [Theory]
    [InlineData("${{ DEPENDENCY_UPDATES_PBI }}", "12345")]
    [InlineData("${{DEPENDENCY_UPDATES_PBI}}", "12345")]
    [InlineData("67890", "67890")]
    [InlineData("Sprint 42", "Sprint 42")]
    public void PopulateEnvironmentVariables_Milestone_ResolvesPlaceholderAndKeepsPlainValue(string milestone, string expected)
    {
        // Arrange
        var envVariableProvider = Substitute.For<IEnvVariableProvider>();
        envVariableProvider.GetEnvironmentVariable("DEPENDENCY_UPDATES_PBI")
            .Returns("12345");

        // Act
        var response = envVariableProvider.PopulateEnvironmentVariables(milestone);

        // Assert
        Assert.Equal(expected, response);
    }

    [Fact]
    public void PopulateEnvironmentVariables_WithUnsetVariable_Throws()
    {
        // Arrange
        var envVariableProvider = Substitute.For<IEnvVariableProvider>();
        envVariableProvider.GetEnvironmentVariable("DEPENDENCY_UPDATES_PBI")
            .Returns((string?) null);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => envVariableProvider.PopulateEnvironmentVariables("${{ DEPENDENCY_UPDATES_PBI }}")
        );
        Assert.Contains("DEPENDENCY_UPDATES_PBI", exception.Message, StringComparison.Ordinal);
    }
}
