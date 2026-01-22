using Aviationexam.DependencyUpdater.Repository.GitHub;
using System;
using System.CommandLine;

namespace Aviationexam.DependencyUpdater;

public sealed class GitHubConfigurationBinder(
    Option<string> owner,
    Option<string> repository,
    Option<string> token,
    Option<string?> authenticationProxyAddress
) : IBinder<GitHubConfiguration>
{
    public GitHubConfiguration CreateValue(
        ParseResult parseResult
    ) => new()
    {
        Owner = parseResult.GetRequiredValue(owner),
        Repository = parseResult.GetRequiredValue(repository),
        Token = parseResult.GetRequiredValue(token),
        AuthenticationProxyAddress = ValidateProxyAddress(parseResult.GetValue(authenticationProxyAddress)),
    };

    private static Uri? ValidateProxyAddress(string? address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return null;
        }

        if (!Uri.TryCreate(address, UriKind.Absolute, out var uri))
        {
            throw new ArgumentException($"Invalid proxy address: {address}. Must be a valid HTTP(S) URL.");
        }

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            throw new ArgumentException($"Invalid proxy address scheme: {uri.Scheme}. Must be HTTP or HTTPS.");
        }

        return uri;
    }
}
