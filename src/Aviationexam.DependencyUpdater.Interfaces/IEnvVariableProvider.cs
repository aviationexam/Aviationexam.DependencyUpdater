namespace Aviationexam.DependencyUpdater.Interfaces;

public interface IEnvVariableProvider
{
    string? GetEnvironmentVariable(string variable);
}
