namespace Aviationexam.DependencyUpdater.Nuget;

public sealed record NugetFile(
    string FullPath,
    ENugetFileType Type
);
