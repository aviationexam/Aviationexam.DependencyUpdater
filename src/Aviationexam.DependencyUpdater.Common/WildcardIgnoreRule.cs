namespace Aviationexam.DependencyUpdater.Common;

public record WildcardIgnoreRule(
    string DependencyPrefix,
    IReadOnlyCollection<string> UpdateTypes
) : IIgnoreRule;
