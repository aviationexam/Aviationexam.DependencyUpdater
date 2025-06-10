using NuGet.Versioning;

namespace Aviationexam.DependencyUpdater.Nuget.Models;

public sealed record NugetPackageVersion(
    string Name,
    NuGetVersion Version
) : INugetPackage
{
    public NugetPackageVersion(
        string Name,
        string Version
    ) : this(Name, new NuGetVersion(Version))
    {
    }
};
