using System.Collections.Generic;

namespace Aviationexam.DependencyUpdater.Common;

public record IgnoreEntry(
    string? DependencyName,
    IReadOnlyCollection<string> UpdateTypes
);
