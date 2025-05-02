namespace Aviationexam.DependencyUpdater.Common;

public record WildcardIgnoreRule(
    string DependencyPrefix
) : IIgnoreRule;
