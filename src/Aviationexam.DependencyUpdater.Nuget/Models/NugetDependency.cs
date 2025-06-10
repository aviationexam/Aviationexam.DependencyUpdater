using System;
using System.Collections.Generic;
using System.Linq;

namespace Aviationexam.DependencyUpdater.Nuget.Models;

public sealed record NugetDependency(
    NugetFile NugetFile,
    INugetPackage NugetPackage,
    IReadOnlyCollection<NugetTargetFramework> TargetFrameworks
)
{
    public bool Equals(NugetDependency? other)
    {
        if (ReferenceEquals(this, other)) return true;
        if (other is null) return false;

        return NugetFile == other.NugetFile
               && NugetPackage.Equals(other.NugetPackage)
               && TargetFrameworks.SequenceEqual(other.TargetFrameworks);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(NugetFile);
        hash.Add(NugetPackage);
        foreach (var mapping in TargetFrameworks)
        {
            hash.Add(mapping);
        }

        return hash.ToHashCode();
    }
}
