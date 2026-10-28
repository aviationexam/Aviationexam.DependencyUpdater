using NuGet.Versioning;

namespace Aviationexam.DependencyUpdater.Nuget.Models;

public sealed record NugetRegistrationLeaf(
    NuGetVersion Version,
    bool Listed,
    string? ReleaseNotes,
    string? ProjectUrl = null
);
