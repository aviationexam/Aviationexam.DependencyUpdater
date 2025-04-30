namespace Aviationexam.DependencyUpdater.Common;

public record GroupEntry(
    string GroupName,
    IReadOnlyCollection<string> Patterns
);
