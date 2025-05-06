namespace Aviationexam.DependencyUpdater.Common;

public sealed record GroupEntry(
    string GroupName,
    IReadOnlyCollection<string> Patterns
);
