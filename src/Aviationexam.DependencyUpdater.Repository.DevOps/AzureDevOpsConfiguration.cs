using Aviationexam.DependencyUpdater.Repository.Abstractions;
using System;

namespace Aviationexam.DependencyUpdater.Repository.DevOps;

public sealed class AzureDevOpsConfiguration : RepositoryPlatformConfiguration
{
    public override string PlatformName => "azure-devops";

    public Uri OrganizationEndpoint => new($"https://dev.azure.com/{Organization}", UriKind.Absolute);

    public required string Organization { get; set; }

    public required string Project { get; set; }

    public required string Repository { get; set; }

    public required string PersonalAccessToken { get; set; }

    public required string AccountId { get; set; }
}
