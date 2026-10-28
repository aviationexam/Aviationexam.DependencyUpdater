namespace Aviationexam.DependencyUpdater.Interfaces.ReleaseNotes;

public sealed record ProjectRelease(
    string? Name,
    string TagName,
    string? Body
);
