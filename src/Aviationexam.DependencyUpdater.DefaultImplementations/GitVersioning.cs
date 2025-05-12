using Aviationexam.DependencyUpdater.Interfaces;
using System;

namespace Aviationexam.DependencyUpdater.DefaultImplementations;

public class GitVersioning : IGitVersioning
{
    public IGitWorkspace CreateWorkspace(string sourceDirectory, string targetDirectory, string branchName)
    {
        throw new NotImplementedException();
    }
}
