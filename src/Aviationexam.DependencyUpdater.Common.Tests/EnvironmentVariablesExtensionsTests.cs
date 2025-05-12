using Aviationexam.DependencyUpdater.Interfaces;
using NSubstitute;
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
}
