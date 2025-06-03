namespace Aviationexam.DependencyUpdater.Common;

public sealed record Optional<T>(
    T? Value
) where T : class;
