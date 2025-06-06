using System;

namespace Aviationexam.DependencyUpdater.Interfaces;

public interface ISourceVersioningWorkspace : IDisposable
{
    string GetWorkspaceDirectory();

    string GetBranchName();

    bool IsPathInsideRepository(
        string fullPath
    );

    void UpdateSubmodule(
        string submodule,
        string branch
    );

    bool HasUncommitedChanges();

    void CommitChanges(
        string message,
        string authorName,
        string authorEmail
    );

    void TryPullRebase(
        string? sourceBranchName,
        string authorName,
        string authorEmail
    );

    bool Push(
        string? sourceBranchName
    );
}
