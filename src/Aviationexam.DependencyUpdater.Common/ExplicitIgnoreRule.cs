namespace Aviationexam.DependencyUpdater.Common;

public record ExplicitIgnoreRule(
    string DependencyName,
    IReadOnlyCollection<string> UpdateTypes
) : IIgnoreRule;
