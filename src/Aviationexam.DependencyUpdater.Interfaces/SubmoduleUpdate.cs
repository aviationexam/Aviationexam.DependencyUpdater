using System.Collections.Generic;

namespace Aviationexam.DependencyUpdater.Interfaces;

/// <param name="FromCommit">Submodule commit referenced by the source branch; null when the source branch does not contain the submodule.</param>
/// <param name="Commits">Commits reachable from <paramref name="ToCommit"/> but not from <paramref name="FromCommit"/>, newest first.</param>
public sealed record SubmoduleUpdate(
    string Path,
    string? RemoteUrl,
    string? FromCommit,
    string ToCommit,
    IReadOnlyList<SubmoduleCommit> Commits
);
