using NuGet.Versioning;

namespace Aviationexam.DependencyUpdater.Nuget;

public sealed record NugetPackageReference(
    string Name,
    VersionRange? VersionRange
) : INugetPackage;
