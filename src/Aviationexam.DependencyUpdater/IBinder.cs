using System.CommandLine;

namespace Aviationexam.DependencyUpdater;

public interface IBinder<TValue>
    where TValue : class
{
    TValue CreateValue(ParseResult parseResult);
}
