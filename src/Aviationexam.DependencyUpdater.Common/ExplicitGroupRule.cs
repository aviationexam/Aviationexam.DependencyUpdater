namespace Aviationexam.DependencyUpdater.Common;

public sealed record ExplicitGroupRule(
    string DependencyName,
    GroupEntry GroupEntry
) : IGroupRule;
