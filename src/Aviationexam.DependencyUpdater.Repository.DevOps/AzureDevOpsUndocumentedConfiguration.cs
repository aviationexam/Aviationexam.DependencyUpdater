namespace Aviationexam.DependencyUpdater.Repository.DevOps;

public sealed class AzureDevOpsUndocumentedConfiguration
{
    public required string NugetFeedProject { get; set; }

    public required string NugetFeedId { get; set; }

    public required string NugetServiceHost { get; set; }

    public required string AccessTokenResourceId { get; set; }
}
