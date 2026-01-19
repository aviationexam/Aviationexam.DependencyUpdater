using NuGet.Versioning;

namespace Aviationexam.DependencyUpdater.Nuget.Models;

public sealed record NugetPackageVersion(
    string Name,
    NuGetVersion Version,
    string? Condition = null
) : INugetPackage
{
    public NugetPackageVersion(
        string Name,
        string Version,
        string? Condition = null
    ) : this(Name, new NuGetVersion(Version), Condition)
    {
    }
};
