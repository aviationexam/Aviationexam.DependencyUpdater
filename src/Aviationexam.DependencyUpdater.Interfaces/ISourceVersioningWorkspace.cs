using System;

namespace Aviationexam.DependencyUpdater.Interfaces;

public interface ISourceVersioningWorkspace : IDisposable
{
    string GetWorkspaceDirectory();

    string GetBranchName();

    bool IsPathInsideRepository(
        string fullPath
    );

    bool HasUncommitedChanges();

    void CommitChanges(
        string message,
        string authorName,
        string authorEmail
    );

    void Push();
}
