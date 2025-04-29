using Aviationexam.DependencyUpdater.Interfaces;
using System.Text.RegularExpressions;

namespace Aviationexam.DependencyUpdater.Common;

public static partial class EnvironmentVariablesExtensions
{
    [GeneratedRegex(@"\${{\s*(?<env>\w+)\s*}}", RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 100)]
    private static partial Regex PlaceholderRegexFactory();

    private static readonly Regex PlaceholderRegex = PlaceholderRegexFactory();

    public static string PopulateEnvironmentVariables(
        this IEnvVariableProvider envVariableProvider, string input
    ) => PlaceholderRegex.Replace(input, match =>
    {
        var envVarName = match.Groups["env"].Value;
        var envVarValue = envVariableProvider.GetEnvironmentVariable(envVarName);

        if (string.IsNullOrEmpty(envVarValue))
        {
            throw new InvalidOperationException($"Environment variable '{envVarName}' is not set but required.");
        }

        return envVarValue;
    });
}
