using System.Threading;
using System.Threading.Tasks;

namespace Aviationexam.DependencyUpdater;

public interface ICommandHandler
{
    Task<int> ExecuteAsync(
        CancellationToken cancellationToken
    );
}
