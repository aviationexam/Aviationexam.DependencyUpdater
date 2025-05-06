namespace Aviationexam.DependencyUpdater.Common;

public sealed record WildcardGroupRule(
    string DependencyPrefix,
    GroupEntry GroupEntry
) : IGroupRule;
