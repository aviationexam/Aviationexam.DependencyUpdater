namespace Aviationexam.DependencyUpdater.Nuget;

public static class NugetPackageSourceMapExtensions
{
    public static bool IsWildcard(
        this NugetPackageSourceMap context
    ) => context.Pattern.Contains('*');
}
