namespace Aviationexam.DependencyUpdater.Nuget.Models;

internal sealed class VssNugetExternalFeedEndpointCredential
{
    public required string Endpoint { get; init; }

    public required string Username { get; init; }

    public required string Password { get; init; }
}
