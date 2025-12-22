using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using ZLinq;

namespace Aviationexam.DependencyUpdater.Common;

public record PackageVersion(
    Version Version,
    bool IsPrerelease,
    IReadOnlyCollection<string> ReleaseLabels,
    IComparer<IReadOnlyCollection<string>> ReleaseLabelsComparer
) : IComparable<PackageVersion>
{
    public int CompareTo(PackageVersion? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other is null) return 1;

        var versionComparison = Version.CompareTo(other.Version);
        if (versionComparison != 0)
        {
            return versionComparison;
        }

        if (IsPrerelease && !other.IsPrerelease) return -1;
        if (!IsPrerelease && other.IsPrerelease) return 1;

        return ReleaseLabelsComparer.Compare(ReleaseLabels, other.ReleaseLabels);
    }

    public static bool operator <(PackageVersion a, PackageVersion b) => a.CompareTo(b) < 0;
    public static bool operator <=(PackageVersion a, PackageVersion b) => a.CompareTo(b) <= 0;
    public static bool operator >(PackageVersion a, PackageVersion b) => a.CompareTo(b) > 0;
    public static bool operator >=(PackageVersion a, PackageVersion b) => a.CompareTo(b) >= 0;

    public virtual bool Equals(PackageVersion? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return Version.Equals(other.Version)
               && IsPrerelease == other.IsPrerelease
               && ReleaseLabels.AsValueEnumerable().SequenceEqual(other.ReleaseLabels);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Version);
        hash.Add(IsPrerelease);
        foreach (var releaseLabel in ReleaseLabels)
        {
            hash.Add(releaseLabel);
        }

        return hash.ToHashCode();
    }

    protected virtual bool PrintMembers(StringBuilder builder)
    {
        RuntimeHelpers.EnsureSufficientExecutionStack();
        builder.Append("Version = ");
        builder.Append(Version);
        builder.Append(", IsPrerelease = ");
        builder.Append(IsPrerelease);
        builder.Append(", ReleaseLabels = ");
        builder.AppendJoin('.', ReleaseLabels);
        return true;
    }

    public string GetSerializedVersion()
    {
        var builder = new StringBuilder();

        if (Version.Revision > 0)
        {
            builder.Append(Version);
        }
        else
        {
            builder.Append(Version.ToString(3));
        }

        if (ReleaseLabels.Count > 0)
        {
            builder.Append('-');
            builder.AppendJoin('.', ReleaseLabels);
        }

        return builder.ToString();
    }
}

public record PackageVersion<TOriginalReference> : PackageVersion
{
    public PackageVersion(
        PackageVersion packageVersion,
        IReadOnlyDictionary<EPackageSource, TOriginalReference> OriginalReference
    ) : base(
        packageVersion.Version,
        packageVersion.IsPrerelease,
        packageVersion.ReleaseLabels,
        packageVersion.ReleaseLabelsComparer
    )
    {
        this.OriginalReference = OriginalReference;
    }

    public virtual bool Equals(
        PackageVersion<TOriginalReference>? other
    ) => base.Equals(other);

    public override int GetHashCode() => base.GetHashCode();

    protected override bool PrintMembers(
        StringBuilder builder
    ) => base.PrintMembers(builder);

    public IReadOnlyDictionary<EPackageSource, TOriginalReference> OriginalReference { get; init; }

    public required IReadOnlyDictionary<EPackageSource, IReadOnlyCollection<DependencySet>> DependencySets { get; init; }
}
