namespace Aviationexam.DependencyUpdater.Repository.DevOps;

public sealed class DevOpsConfiguration
{
    public required string Organization { get; set; }

    public required string PersonalAccessToken { get; set; }

    public required string Project { get; set; }

    public required string Repository { get; set; }
}
