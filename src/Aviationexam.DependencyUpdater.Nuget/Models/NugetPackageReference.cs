using NuGet.Versioning;

namespace Aviationexam.DependencyUpdater.Nuget.Models;

public sealed record NugetPackageReference(
    string Name,
    VersionRange? VersionRange,
    string? Condition = null
) : INugetPackage;
