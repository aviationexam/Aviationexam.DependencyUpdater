using System.Collections.Generic;

namespace Aviationexam.DependencyUpdater.Nuget.Models;

public sealed record NugetRegistrationIndex(
    IReadOnlyCollection<NugetRegistrationLeaf> Leaves,
    IReadOnlyCollection<string> ExternalPageUrls
);
