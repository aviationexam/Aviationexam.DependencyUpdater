using System.Collections.Generic;

namespace Aviationexam.DependencyUpdater.Repository.DevOps.Dtos;

internal sealed class LocalInstance
{
    public required IReadOnlyCollection<SourceChainItem> SourceChain { get; set; }
    public required int Origin { get; set; }
    public required bool IsDeleted { get; set; }
}
