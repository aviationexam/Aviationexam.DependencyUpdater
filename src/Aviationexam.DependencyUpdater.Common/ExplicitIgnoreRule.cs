namespace Aviationexam.DependencyUpdater.Common;

public record ExplicitIgnoreRule(
    string DependencyName
) : IIgnoreRule;
