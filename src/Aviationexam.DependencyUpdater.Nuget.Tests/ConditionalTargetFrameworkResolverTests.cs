using Aviationexam.DependencyUpdater.Nuget.Helpers;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using Xunit;

namespace Aviationexam.DependencyUpdater.Nuget.Tests;

public class ConditionalTargetFrameworkResolverTests
{
    private readonly ILogger<ConditionalTargetFrameworkResolver> _logger = Substitute.For<ILogger<ConditionalTargetFrameworkResolver>>();

    private ConditionalTargetFrameworkResolver CreateResolver() => new(_logger);

    [Theory]
    [InlineData("'$(TargetFramework)' == 'net9.0'", "net9.0")]
    [InlineData("'$(TargetFramework)' == 'net8.0'", "net8.0")]
    [InlineData("'$(TargetFramework)' == 'netstandard2.0'", "netstandard2.0")]
    [InlineData("'$(TargetFramework)' == 'net472'", "net472")]
    public void TryExtractTargetFramework_QuotedVariable_ReturnsTrue(string condition, string expectedTfm)
    {
        // Act
        var result = ConditionalTargetFrameworkResolver.TryExtractTargetFramework(condition, out var targetFramework);

        // Assert
        Assert.True(result);
        Assert.Equal(expectedTfm, targetFramework);
    }

    [Theory]
    [InlineData("'$(TargetFramework)'=='net9.0'", "net9.0")]
    [InlineData("'$(TargetFramework)'=='netstandard2.1'", "netstandard2.1")]
    public void TryExtractTargetFramework_QuotedVariable_NoSpaces_ReturnsTrue(string condition, string expectedTfm)
    {
        // Act
        var result = ConditionalTargetFrameworkResolver.TryExtractTargetFramework(condition, out var targetFramework);

        // Assert
        Assert.True(result);
        Assert.Equal(expectedTfm, targetFramework);
    }

    [Theory]
    [InlineData("$(TargetFramework) == 'net9.0'", "net9.0")]
    [InlineData("$(TargetFramework) == 'net8.0'", "net8.0")]
    [InlineData("$(TargetFramework) == 'netstandard2.0'", "netstandard2.0")]
    [InlineData("$(TargetFramework) == 'net472'", "net472")]
    [InlineData("$(TargetFramework) == 'netstandard2.1'", "netstandard2.1")]
    public void TryExtractTargetFramework_UnquotedVariable_ReturnsTrue(string condition, string expectedTfm)
    {
        // Act
        var result = ConditionalTargetFrameworkResolver.TryExtractTargetFramework(condition, out var targetFramework);

        // Assert
        Assert.True(result);
        Assert.Equal(expectedTfm, targetFramework);
    }

    [Theory]
    [InlineData("$(TargetFramework)=='net9.0'", "net9.0")]
    [InlineData("$(TargetFramework)=='netstandard2.0'", "netstandard2.0")]
    public void TryExtractTargetFramework_UnquotedVariable_NoSpaces_ReturnsTrue(string condition, string expectedTfm)
    {
        // Act
        var result = ConditionalTargetFrameworkResolver.TryExtractTargetFramework(condition, out var targetFramework);

        // Assert
        Assert.True(result);
        Assert.Equal(expectedTfm, targetFramework);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TryExtractTargetFramework_NullOrWhitespace_ReturnsFalse(string? condition)
    {
        // Act
        var result = ConditionalTargetFrameworkResolver.TryExtractTargetFramework(condition, out var targetFramework);

        // Assert
        Assert.False(result);
        Assert.Null(targetFramework);
    }

    [Theory]
    [InlineData("$(Configuration) == 'Release'")]
    [InlineData("'$(Platform)' == 'AnyCPU'")]
    [InlineData("SomeRandomCondition")]
    [InlineData("$(TargetFramework) != 'net9.0'")]
    public void TryExtractTargetFramework_InvalidCondition_ReturnsFalse(string condition)
    {
        // Act
        var result = ConditionalTargetFrameworkResolver.TryExtractTargetFramework(condition, out var targetFramework);

        // Assert
        Assert.False(result);
        Assert.Null(targetFramework);
    }

    [Theory]
    [InlineData("'$(TARGETFRAMEWORK)' == 'net9.0'", "net9.0")]
    [InlineData("'$(targetframework)' == 'net8.0'", "net8.0")]
    [InlineData("$(TARGETFRAMEWORK) == 'netstandard2.0'", "netstandard2.0")]
    public void TryExtractTargetFramework_CaseInsensitive_ReturnsTrue(string condition, string expectedTfm)
    {
        // Act
        var result = ConditionalTargetFrameworkResolver.TryExtractTargetFramework(condition, out var targetFramework);

        // Assert
        Assert.True(result);
        Assert.Equal(expectedTfm, targetFramework);
    }

    [Fact]
    public void Resolve_EmptyConditions_ReturnsNull()
    {
        // Arrange
        var resolver = CreateResolver();

        // Act
        var result = resolver.Resolve([], "TestPackage");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Resolve_SingleTargetFrameworkCondition_ReturnsFramework()
    {
        // Arrange
        var resolver = CreateResolver();
        string[] conditions = ["'$(TargetFramework)' == 'net9.0'"];

        // Act
        var result = resolver.Resolve(conditions, "TestPackage");

        // Assert
        Assert.Equal("net9.0", result);
    }

    [Fact]
    public void Resolve_FiltersOutNonTargetFrameworkConditions()
    {
        // Arrange
        var resolver = CreateResolver();
        string[] conditions =
        [
            "$(UseTestingPlatform) != 'true'",
            "'$(TargetFramework)' == 'net9.0'",
        ];

        // Act
        var result = resolver.Resolve(conditions, "TestPackage");

        // Assert
        Assert.Equal("net9.0", result);
    }

    [Fact]
    public void Resolve_OnlyNonTargetFrameworkConditions_ReturnsNull()
    {
        // Arrange
        var resolver = CreateResolver();
        string[] conditions =
        [
            "$(UseTestingPlatform) != 'true'",
            "$(Configuration) == 'Release'",
        ];

        // Act
        var result = resolver.Resolve(conditions, "TestPackage");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Resolve_MultipleTargetFrameworkConditions_ThrowsInvalidOperationException()
    {
        // Arrange
        var resolver = CreateResolver();
        string[] conditions =
        [
            "'$(TargetFramework)' == 'net9.0'",
            "'$(TargetFramework)' == 'net8.0'",
        ];

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => resolver.Resolve(conditions, "TestPackage")
        );
        Assert.Contains("Multiple target framework conditions found", exception.Message);
        Assert.Contains("TestPackage", exception.Message);
        Assert.Contains("net9.0", exception.Message);
        Assert.Contains("net8.0", exception.Message);
    }

    [Fact]
    public void Resolve_ConditionContainsTargetFrameworkButInvalidFormat_ReturnsNull()
    {
        // Arrange
        var resolver = CreateResolver();
        string[] conditions = ["$(TargetFramework) != 'net9.0'"];

        // Act
        var result = resolver.Resolve(conditions, "TestPackage");

        // Assert
        Assert.Null(result);
    }
}
