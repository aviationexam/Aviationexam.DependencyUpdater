using Aviationexam.DependencyUpdater.Common;
using NuGet.Packaging;
using NuGet.Protocol;
using System.Collections.Generic;

namespace Aviationexam.DependencyUpdater.Nuget.Models;

public record PossiblePackageVersion(
    PackageVersion<PackageSearchMetadataRegistration> PackageVersion,
    IReadOnlyCollection<PackageDependencyGroup> CompatiblePackageDependencyGroups
);
