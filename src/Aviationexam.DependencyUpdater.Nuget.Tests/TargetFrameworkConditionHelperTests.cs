using Aviationexam.DependencyUpdater.Nuget.Helpers;
using Xunit;

namespace Aviationexam.DependencyUpdater.Nuget.Tests;

public class TargetFrameworkConditionHelperTests
{
    [Theory]
    [InlineData("'$(TargetFramework)' == 'net9.0'", "net9.0")]
    [InlineData("'$(TargetFramework)' == 'net8.0'", "net8.0")]
    [InlineData("'$(TargetFramework)' == 'netstandard2.0'", "netstandard2.0")]
    [InlineData("'$(TargetFramework)' == 'net472'", "net472")]
    public void TryExtractTargetFramework_QuotedVariable_ReturnsTrue(string condition, string expectedTfm)
    {
        // Act
        var result = TargetFrameworkConditionHelper.TryExtractTargetFramework(condition, out var targetFramework);

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
        var result = TargetFrameworkConditionHelper.TryExtractTargetFramework(condition, out var targetFramework);

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
        var result = TargetFrameworkConditionHelper.TryExtractTargetFramework(condition, out var targetFramework);

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
        var result = TargetFrameworkConditionHelper.TryExtractTargetFramework(condition, out var targetFramework);

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
        var result = TargetFrameworkConditionHelper.TryExtractTargetFramework(condition, out var targetFramework);

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
        var result = TargetFrameworkConditionHelper.TryExtractTargetFramework(condition, out var targetFramework);

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
        var result = TargetFrameworkConditionHelper.TryExtractTargetFramework(condition, out var targetFramework);

        // Assert
        Assert.True(result);
        Assert.Equal(expectedTfm, targetFramework);
    }
}
