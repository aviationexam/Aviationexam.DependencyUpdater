namespace Aviationexam.DependencyUpdater.Common;

public sealed record ExplicitIgnoreRule(
    string DependencyName,
    IReadOnlyCollection<string> UpdateTypes
) : IIgnoreRule;
