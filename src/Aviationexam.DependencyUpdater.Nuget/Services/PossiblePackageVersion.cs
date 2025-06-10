using Aviationexam.DependencyUpdater.Common;
using NuGet.Protocol;
using System.Collections.Generic;

namespace Aviationexam.DependencyUpdater.Nuget.Services;

public record PossiblePackageVersion(
    PackageVersion<PackageSearchMetadataRegistration> PackageVersion,
    IReadOnlyCollection<NuGet.Packaging.PackageDependencyGroup> CompatiblePackageDependencyGroups
);
