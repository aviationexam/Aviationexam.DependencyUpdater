using System.Collections.Generic;

namespace Aviationexam.DependencyUpdater.Nuget.Dtos;

internal sealed class VssNugetExternalFeedEndpoints
{
    public required IReadOnlyCollection<VssNugetExternalFeedEndpointCredential> EndpointCredentials { get; init; }
}
