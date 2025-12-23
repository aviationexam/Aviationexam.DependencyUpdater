using NuGet.Packaging.Core;
using PackageDependencyInfo = Aviationexam.DependencyUpdater.Common.PackageDependencyInfo;

namespace Aviationexam.DependencyUpdater.Nuget.Extensions;

public static class PackageDependencyExtensions
{
    public static PackageDependencyInfo MapToPackageDependencyInfo(
        this PackageDependency packageDependency
    ) => new(
        packageDependency.Id,
        MinVersion: packageDependency.VersionRange.MinVersion?.MapToPackageVersion(),
        IncludeMinVersion: packageDependency.VersionRange.IsMinInclusive,
        MaxVersion: packageDependency.VersionRange.MaxVersion?.MapToPackageVersion(),
        IncludeMaxVersion: packageDependency.VersionRange.IsMaxInclusive,
        FloatRangeVersion: packageDependency.VersionRange.Float?.ToString(),
        OriginalVersionString: packageDependency.VersionRange.OriginalString
    );
}
