using Aviationexam.DependencyUpdater.Nuget.Models;

namespace Aviationexam.DependencyUpdater.Nuget.Extensions;

public static class NugetPackageSourceMapExtensions
{
    public static bool IsWildcard(
        this NugetPackageSourceMap context
    ) => context.Pattern.Contains('*');
}
