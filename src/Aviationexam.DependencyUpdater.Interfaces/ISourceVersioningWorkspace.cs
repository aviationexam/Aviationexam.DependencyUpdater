using System;

namespace Aviationexam.DependencyUpdater.Interfaces;

public interface ISourceVersioningWorkspace : IDisposable
{
    string GetWorkspaceDirectory();

    bool HasUncommitedChanges();

    void CommitChanges(string message);

    void Push();
}
