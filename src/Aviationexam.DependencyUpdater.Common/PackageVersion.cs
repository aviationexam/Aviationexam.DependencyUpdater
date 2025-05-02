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
               && ReleaseLabels.SequenceEqual(other.ReleaseLabels);
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
}

public record PackageVersion<TOriginalReference>(
    Version Version,
    bool IsPrerelease,
    IReadOnlyCollection<string> ReleaseLabels,
    IComparer<IReadOnlyCollection<string>> ReleaseLabelsComparable,
    TOriginalReference OriginalReference
) : PackageVersion(
    Version,
    IsPrerelease,
    ReleaseLabels,
    ReleaseLabelsComparable
);
