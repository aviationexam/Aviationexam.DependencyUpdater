namespace Aviationexam.DependencyUpdater.Interfaces;

public interface IGitVersioning
{
    IGitWorkspace CreateWorkspace(
        string sourceDirectory, string targetDirectory, string branchName
    );
}
