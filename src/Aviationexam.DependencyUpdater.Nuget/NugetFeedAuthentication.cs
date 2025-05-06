namespace Aviationexam.DependencyUpdater.Nuget;

public sealed record NugetFeedAuthentication(
    string Key,
    string FeedUrl,
    string? Username,
    string? Password
);
