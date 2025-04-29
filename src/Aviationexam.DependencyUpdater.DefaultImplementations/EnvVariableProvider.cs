using Aviationexam.DependencyUpdater.Interfaces;
using System;

namespace Aviationexam.DependencyUpdater.DefaultImplementations;

public class EnvVariableProvider : IEnvVariableProvider
{
    public string? GetEnvironmentVariable(
        string variable
    ) => Environment.GetEnvironmentVariable(variable);
}
