using System;

namespace Aviationexam.DependencyUpdater.Repository.DevOps.Dtos;

internal sealed class AzSideCarResponse
{
    public required string Token { get; init; }

    public required DateTimeOffset ExpiresOn { get; init; }
}
