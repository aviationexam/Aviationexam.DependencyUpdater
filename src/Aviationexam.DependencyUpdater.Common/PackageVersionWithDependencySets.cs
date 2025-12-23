using System.Collections.Generic;
using System.Text;

namespace Aviationexam.DependencyUpdater.Common;

public record PackageVersionWithDependencySets : PackageVersion
{
    public PackageVersionWithDependencySets(
        PackageVersion packageVersion
    ) : base(
        packageVersion.Version,
        packageVersion.IsPrerelease,
        packageVersion.ReleaseLabels,
        packageVersion.ReleaseLabelsComparer
    )
    {
    }

    public virtual bool Equals(
        PackageVersionWithDependencySets? other
    ) => base.Equals(other);

    public override int GetHashCode() => base.GetHashCode();

    protected override bool PrintMembers(
        StringBuilder builder
    ) => base.PrintMembers(builder);

    public required IReadOnlyDictionary<EPackageSource, IReadOnlyCollection<DependencySet>> DependencySets { get; init; }
}
