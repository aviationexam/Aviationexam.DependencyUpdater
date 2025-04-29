namespace Aviationexam.DependencyUpdater.Nuget;

public sealed record NugetFeedAuthentication(
    string FeedUrl,
    string? Username,
    string? Password
);
