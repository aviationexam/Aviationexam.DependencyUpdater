using System.Collections.Generic;

namespace Aviationexam.DependencyUpdater.Common;

public sealed record GroupEntry(
    string GroupName,
    IReadOnlyCollection<string> Patterns
);
