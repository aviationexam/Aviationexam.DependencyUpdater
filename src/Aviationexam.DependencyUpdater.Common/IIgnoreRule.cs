namespace Aviationexam.DependencyUpdater.Common;

public interface IIgnoreRule
{
    IReadOnlyCollection<string> UpdateTypes { get; }
}
