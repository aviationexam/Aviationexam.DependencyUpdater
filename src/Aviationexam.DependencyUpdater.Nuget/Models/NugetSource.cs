using System;
using System.Collections.Immutable;
using ZLinq;

namespace Aviationexam.DependencyUpdater.Nuget.Models;

public sealed record NugetSource(
    string Name,
    string Source,
    NugetSourceVersion Version,
    ImmutableArray<NugetPackageSourceMap> PackageMapping
)
{
    public bool Equals(NugetSource? other)
    {
        if (ReferenceEquals(this, other)) return true;
        if (other is null) return false;

        return Name == other.Name
               && Source == other.Source
               && Version == other.Version
               && PackageMapping.AsValueEnumerable().SequenceEqual(other.PackageMapping);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Name);
        hash.Add(Source);
        hash.Add(Version);
        foreach (var mapping in PackageMapping)
        {
            hash.Add(mapping);
        }

        return hash.ToHashCode();
    }
}
