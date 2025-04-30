namespace Aviationexam.DependencyUpdater.Common;

public record PackageVersion<TOriginalReference>(
    Version Version,
    bool IsPrerelease,
    TOriginalReference OriginalReference
);
