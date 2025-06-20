using Aviationexam.DependencyUpdater.Interfaces;
using System.CommandLine;

namespace Aviationexam.DependencyUpdater;

public sealed class GitCredentialsConfigurationBinder(
    Option<string> usernameArgument,
    Option<string> passwordArgument
) : IBinder<GitCredentialsConfiguration>
{
    public GitCredentialsConfiguration CreateValue(
        ParseResult parseResult
        ) => new()
    {
        Username = parseResult.GetRequiredValue(usernameArgument),
        Password = parseResult.GetRequiredValue(passwordArgument),
    };
}
