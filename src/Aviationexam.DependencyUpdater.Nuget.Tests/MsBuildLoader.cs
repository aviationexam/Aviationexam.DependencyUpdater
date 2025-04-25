using Microsoft.Build.Locator;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Aviationexam.DependencyUpdater.Nuget.Tests;

internal static class MsBuildLoader
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        if (!MSBuildLocator.IsRegistered)
        {
            var instance = MSBuildLocator.QueryVisualStudioInstances()
                .OrderByDescending(i => i.Version)
                .First();

            MSBuildLocator.RegisterInstance(instance);
        }
    }
}
