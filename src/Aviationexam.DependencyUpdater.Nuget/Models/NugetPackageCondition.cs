namespace Aviationexam.DependencyUpdater.Nuget.Models;

public sealed record NugetPackageCondition(
    string Condition
)
{
    public static readonly NugetPackageCondition WithoutCondition = new(string.Empty);
}
