using System.Threading;
using System.Threading.Tasks;

namespace Aviationexam.DependencyUpdater.Interfaces.Repository;

public interface IPackageFeedClient
{
    Task EnsurePackageVersionIsAvailableAsync(
        string packageName,
        string packageVersion,
        CancellationToken cancellationToken
    );
}
