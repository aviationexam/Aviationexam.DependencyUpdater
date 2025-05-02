namespace Aviationexam.DependencyUpdater.Common;

public record PackageVersion(
    Version Version,
    bool IsPrerelease,
    IReadOnlyCollection<string> ReleaseLabels
);

public record PackageVersion<TOriginalReference>(
    Version Version,
    bool IsPrerelease,
    IReadOnlyCollection<string> ReleaseLabels,
    TOriginalReference OriginalReference
) : PackageVersion(
    Version,
    IsPrerelease,
    ReleaseLabels
);
