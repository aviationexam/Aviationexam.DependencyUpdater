namespace Aviationexam.DependencyUpdater.Common;

public class IgnoreResolver(
    IReadOnlyCollection<IIgnoreRule> Rules
);
