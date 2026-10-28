namespace Aviationexam.DependencyUpdater.Common;

public sealed record PackageReleaseNote(
    string PackageName,
    string Version,
    string Notes
);
