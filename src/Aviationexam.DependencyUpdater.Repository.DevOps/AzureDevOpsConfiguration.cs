using Aviationexam.DependencyUpdater.Interfaces;
using Aviationexam.DependencyUpdater.Interfaces.Repository;
using System;

namespace Aviationexam.DependencyUpdater.Repository.DevOps;

public sealed class AzureDevOpsConfiguration : IRepositoryPlatformConfiguration
{
    public EPlatformSelection Platform => EPlatformSelection.AzureDevOps;

    public Uri OrganizationEndpoint => new($"https://dev.azure.com/{Organization}", UriKind.Absolute);

    public required string Organization { get; set; }

    public required string Project { get; set; }

    public required string Repository { get; set; }

    public required string PersonalAccessToken { get; set; }

    public required string AccountId { get; set; }
}
