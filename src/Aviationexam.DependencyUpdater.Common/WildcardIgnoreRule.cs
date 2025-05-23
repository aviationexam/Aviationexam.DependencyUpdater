using System.Collections.Generic;

namespace Aviationexam.DependencyUpdater.Common;

public sealed record WildcardIgnoreRule(
    string DependencyPrefix,
    IReadOnlyCollection<string> UpdateTypes
) : IIgnoreRule;
