namespace Aviationexam.DependencyUpdater.Repository.DevOps;

public sealed record DevOpsConfiguration(
    string Organization,
    string PersonalAccessToken,
    string Project,
    string Repository
);
