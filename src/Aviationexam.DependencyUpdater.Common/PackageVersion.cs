namespace Aviationexam.DependencyUpdater.Common;

public abstract record PackageVersion(
    Version Version,
    bool IsPrerelease
);

public record PackageVersion<TOriginalReference>(
    Version Version,
    bool IsPrerelease,
    TOriginalReference OriginalReference
) : PackageVersion(
    Version,
    IsPrerelease
);
