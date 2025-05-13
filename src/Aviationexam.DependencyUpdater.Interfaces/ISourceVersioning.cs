using System;

namespace Aviationexam.DependencyUpdater.Interfaces;

public interface ISourceVersioning : IDisposable
{
    ISourceVersioningWorkspace CreateWorkspace(
        string targetDirectory,
        string branchName,
        string worktreeName
    );
}
