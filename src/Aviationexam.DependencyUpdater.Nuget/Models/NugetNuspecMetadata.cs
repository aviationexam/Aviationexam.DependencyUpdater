namespace Aviationexam.DependencyUpdater.Nuget.Models;

public sealed record NugetNuspecMetadata(
    string? ReleaseNotes,
    string? RepositoryUrl,
    string? ProjectUrl
);
