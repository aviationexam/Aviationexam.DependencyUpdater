using Aviationexam.DependencyUpdater.Nuget.Models;

namespace Aviationexam.DependencyUpdater.Nuget.Configurations;

public sealed record NugetFeedAuthentication(
    string Key,
    string FeedUrl,
    string? Username,
    string? Password,
    NugetSourceVersion Version
);
