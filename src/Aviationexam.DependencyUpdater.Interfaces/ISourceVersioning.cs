using System;

namespace Aviationexam.DependencyUpdater.Interfaces;

public interface ISourceVersioning : IDisposable
{
    ISourceVersioningWorkspace CreateWorkspace(
        string sourceDirectory, string targetDirectory, string branchName
    );
}
