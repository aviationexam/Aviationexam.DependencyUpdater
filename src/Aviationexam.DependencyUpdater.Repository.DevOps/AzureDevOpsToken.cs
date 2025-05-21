using System;

namespace Aviationexam.DependencyUpdater.Repository.DevOps;

internal sealed record AzureDevOpsToken(
    string Token,
    DateTimeOffset ExpiresOn
);
