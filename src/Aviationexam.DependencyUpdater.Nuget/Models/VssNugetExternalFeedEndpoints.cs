using System.Collections.Generic;

namespace Aviationexam.DependencyUpdater.Nuget.Models;

internal sealed class VssNugetExternalFeedEndpoints
{
    public required IReadOnlyCollection<VssNugetExternalFeedEndpointCredential> EndpointCredentials { get; init; }
}
