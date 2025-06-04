using System;

namespace Aviationexam.DependencyUpdater.Repository.DevOps.Dtos;

internal sealed class AzSideCarResponse
{
    public required string AccessToken { get; init; }

    public required DateTimeOffset ExpiresOn { get; init; }
}
