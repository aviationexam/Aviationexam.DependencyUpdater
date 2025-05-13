namespace Aviationexam.DependencyUpdater.Interfaces;

public interface ISourceVersioningFactory
{
    ISourceVersioning CreateSourceVersioning(
        string sourceDirectory
    );
}
