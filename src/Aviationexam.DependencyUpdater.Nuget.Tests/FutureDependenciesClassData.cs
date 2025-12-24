using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.Models;
using Aviationexam.DependencyUpdater.Nuget.Services;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using Xunit;

namespace Aviationexam.DependencyUpdater.Nuget.Tests;

public sealed class FutureDependenciesClassData() : TheoryData<
    IReadOnlyCollection<KeyValuePair<NugetDependency, IReadOnlyCollection<PackageVersionWithDependencySets>>>,
    DependencyProcessingResult,
    IReadOnlyDictionary<string, PackageVersionWithDependencySets?>
>(
    (
        [
            KeyValuePair.Create(
                new NugetDependency(
                    new NugetFile("", ENugetFileType.Csproj),
                    new NugetPackageReference("Aviationexam.Core.Common.JsonInfrastructure", VersionRange.Parse("0.1.2558.0")),
                    [new NugetTargetFramework("net48")]
                ),
                (IReadOnlyCollection<PackageVersionWithDependencySets>)
                [
                    new PackageVersionWithDependencySets(CreatePackageVersion("0.1.2562.0"))
                    {
                        DependencySets = CreateDependencySets(
                            new DependencySet("net8.0", [
                                new PackageDependencyInfo("Aviationexam.Core.Common.SharedDTOs", CreatePackageVersion("0.1.2562.0")),
                                new PackageDependencyInfo("System.ComponentModel.Annotations", CreatePackageVersion("5.0.0.0")),
                                new PackageDependencyInfo("System.Text.Json", CreatePackageVersion("8.0.6.0")),
                            ]),
                            new DependencySet("netstandard2.0", [
                                new PackageDependencyInfo("Aviationexam.Core.Common.SharedDTOs", CreatePackageVersion("0.1.2562.0")),
                                new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("8.0.0.0")),
                                new PackageDependencyInfo("System.Buffers", CreatePackageVersion("4.6.1.0")),
                                new PackageDependencyInfo("System.ComponentModel.Annotations", CreatePackageVersion("5.0.0.0")),
                                new PackageDependencyInfo("System.Memory", CreatePackageVersion("4.6.3.0")),
                                new PackageDependencyInfo("System.Numerics.Vectors", CreatePackageVersion("4.6.1.0")),
                                new PackageDependencyInfo("System.Runtime.CompilerServices.Unsafe", CreatePackageVersion("6.1.2.0")),
                                new PackageDependencyInfo("System.Text.Encodings.Web", CreatePackageVersion("8.0.0.0")),
                                new PackageDependencyInfo("System.Text.Json", CreatePackageVersion("8.0.6.0")),
                                new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("4.6.3.0")),
                            ])
                        )
                    }
                ]
            ),
            KeyValuePair.Create(
                new NugetDependency(
                    new NugetFile("", ENugetFileType.Csproj),
                    new NugetPackageReference("Aviationexam.Core.Common.Model", VersionRange.Parse("0.1.2558.0")),
                    [new NugetTargetFramework("net48")]
                ),
                (IReadOnlyCollection<PackageVersionWithDependencySets>)
                [
                    new PackageVersionWithDependencySets(new PackageVersion(new Version("0.1.2562.0"), false, [], NugetReleaseLabelComparer.Instance))
                    {
                        DependencySets = CreateDependencySets(
                            new DependencySet("net8.0", [
                                new PackageDependencyInfo("Aviationexam.Core.Common.SharedDTOs", CreatePackageVersion("0.1.2562.0")),
                                new PackageDependencyInfo("Aviationexam.Core.Common.SharedInterfaces", CreatePackageVersion("0.1.2562.0")),
                                new PackageDependencyInfo("Aviationexam.Core.Common.Validation", CreatePackageVersion("0.1.2562.0")),
                                new PackageDependencyInfo("Microsoft.IdentityModel.JsonWebTokens", CreatePackageVersion("8.15.0.0")),
                                new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("8.0.2.0")),
                                new PackageDependencyInfo("Microsoft.Extensions.Diagnostics.HealthChecks.Abstractions", CreatePackageVersion("8.0.22.0")),
                                new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("8.0.3.0")),
                                new PackageDependencyInfo("Microsoft.IdentityModel.Abstractions", CreatePackageVersion("8.15.0.0")),
                                new PackageDependencyInfo("Microsoft.IdentityModel.Logging", CreatePackageVersion("8.15.0.0")),
                                new PackageDependencyInfo("Microsoft.IdentityModel.Tokens", CreatePackageVersion("8.15.0.0")),
                                new PackageDependencyInfo("System.ComponentModel.Annotations", CreatePackageVersion("5.0.0.0")),
                                new PackageDependencyInfo("System.Memory", CreatePackageVersion("4.6.3.0")),
                                new PackageDependencyInfo("System.Text.Json", CreatePackageVersion("8.0.6.0")),
                            ]),
                            new DependencySet("netstandard2.0", [
                                new PackageDependencyInfo("Aviationexam.Core.Common.SharedDTOs", CreatePackageVersion("0.1.2562.0")),
                                new PackageDependencyInfo("Aviationexam.Core.Common.SharedInterfaces", CreatePackageVersion("0.1.2562.0")),
                                new PackageDependencyInfo("Aviationexam.Core.Common.Validation", CreatePackageVersion("0.1.2562.0")),
                                new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("8.0.0.0")),
                                new PackageDependencyInfo("Microsoft.IdentityModel.JsonWebTokens", CreatePackageVersion("8.15.0.0")),
                                new PackageDependencyInfo("System.Collections.Immutable", CreatePackageVersion("8.0.0.0")),
                                new PackageDependencyInfo("Microsoft.Bcl.TimeProvider", CreatePackageVersion("8.0.1.0")),
                                new PackageDependencyInfo("Microsoft.CSharp", CreatePackageVersion("4.7.0.0")),
                                new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("8.0.2.0")),
                                new PackageDependencyInfo("Microsoft.Extensions.Diagnostics.HealthChecks.Abstractions", CreatePackageVersion("8.0.22.0")),
                                new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("8.0.3.0")),
                                new PackageDependencyInfo("Microsoft.IdentityModel.Abstractions", CreatePackageVersion("8.15.0.0")),
                                new PackageDependencyInfo("Microsoft.IdentityModel.Logging", CreatePackageVersion("8.15.0.0")),
                                new PackageDependencyInfo("Microsoft.IdentityModel.Tokens", CreatePackageVersion("8.15.0.0")),
                                new PackageDependencyInfo("System.Buffers", CreatePackageVersion("4.6.1.0")),
                                new PackageDependencyInfo("System.ComponentModel.Annotations", CreatePackageVersion("5.0.0.0")),
                                new PackageDependencyInfo("System.Diagnostics.DiagnosticSource", CreatePackageVersion("8.0.1.0")),
                                new PackageDependencyInfo("System.Memory", CreatePackageVersion("4.6.3.0")),
                                new PackageDependencyInfo("System.Numerics.Vectors", CreatePackageVersion("4.6.1.0")),
                                new PackageDependencyInfo("System.Runtime.CompilerServices.Unsafe", CreatePackageVersion("6.1.2.0")),
                                new PackageDependencyInfo("System.Security.Cryptography.Cng", CreatePackageVersion("5.0.0.0")),
                                new PackageDependencyInfo("System.Text.Encodings.Web", CreatePackageVersion("8.0.0.0")),
                                new PackageDependencyInfo("System.Text.Json", CreatePackageVersion("8.0.6.0")),
                                new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("4.6.3.0")),
                            ])
                        )
                    },
                ]
            )
        ],
        new DependencyProcessingResult(
            CreatePackageFlags(
                (new Package("Aviationexam.Core.Common.SharedDTOs", CreatePackageVersion("0.1.2562.0")), Net48, EDependencyFlag.Unknown),
                (new Package("System.ComponentModel.Annotations", CreatePackageVersion("5.0.0.0")), Net48, EDependencyFlag.Unknown),
                (new Package("System.Text.Json", CreatePackageVersion("8.0.6.0")), Net48, EDependencyFlag.Unknown),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("8.0.0.0")), Net48, EDependencyFlag.Unknown),
                (new Package("System.Buffers", CreatePackageVersion("4.6.1.0")), Net48, EDependencyFlag.Unknown),
                (new Package("System.Memory", CreatePackageVersion("4.6.3.0")), Net48, EDependencyFlag.Unknown),
                (new Package("System.Numerics.Vectors", CreatePackageVersion("4.6.1.0")), Net48, EDependencyFlag.Unknown),
                (new Package("System.Runtime.CompilerServices.Unsafe", CreatePackageVersion("6.1.2.0")), Net48, EDependencyFlag.Unknown),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("8.0.0.0")), Net48, EDependencyFlag.Unknown),
                (new Package("System.Threading.Tasks.Extensions", CreatePackageVersion("4.6.3.0")), Net48, EDependencyFlag.Unknown),
                (new Package("Aviationexam.Core.Common.SharedInterfaces", CreatePackageVersion("0.1.2562.0")), Net48, EDependencyFlag.Unknown),
                (new Package("Aviationexam.Core.Common.Validation", CreatePackageVersion("0.1.2562.0")), Net48, EDependencyFlag.Unknown),
                (new Package("Microsoft.IdentityModel.JsonWebTokens", CreatePackageVersion("8.15.0.0")), Net48, EDependencyFlag.Unknown),
                (new Package("System.Collections.Immutable", CreatePackageVersion("8.0.0.0")), Net48, EDependencyFlag.Unknown),
                (new Package("Microsoft.Bcl.TimeProvider", CreatePackageVersion("8.0.1.0")), Net48, EDependencyFlag.Unknown),
                (new Package("Microsoft.CSharp", CreatePackageVersion("4.7.0.0")), Net48, EDependencyFlag.Unknown),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("8.0.2.0")), Net48, EDependencyFlag.Unknown),
                (new Package("Microsoft.Extensions.Diagnostics.HealthChecks.Abstractions", CreatePackageVersion("8.0.22.0")), Net48, EDependencyFlag.Unknown),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("8.0.3.0")), Net48, EDependencyFlag.Unknown),
                (new Package("Microsoft.IdentityModel.Abstractions", CreatePackageVersion("8.15.0.0")), Net48, EDependencyFlag.Unknown),
                (new Package("Microsoft.IdentityModel.Logging", CreatePackageVersion("8.15.0.0")), Net48, EDependencyFlag.Unknown),
                (new Package("Microsoft.IdentityModel.Tokens", CreatePackageVersion("8.15.0.0")), Net48, EDependencyFlag.Unknown),
                (new Package("System.Diagnostics.DiagnosticSource", CreatePackageVersion("8.0.1.0")), Net48, EDependencyFlag.Unknown),
                (new Package("System.Security.Cryptography.Cng", CreatePackageVersion("5.0.0.0")), Net48, EDependencyFlag.Unknown)
            ),
            []
        ),
        new Dictionary<string, PackageVersionWithDependencySets?>
        {
            ["Aviationexam.Core.Common.NameOfGenerator.Attributes=0.1.2562.0"] = new(CreatePackageVersion("0.1.2562.0"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("netstandard2.0", [])
                )
            },
            ["Aviationexam.Core.Common.SharedDTOs=0.1.2562.0"] = new(CreatePackageVersion("0.1.2562.0"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("System.ComponentModel.Annotations", CreatePackageVersion("5.0.0.0")),
                        new PackageDependencyInfo("System.Text.Json", CreatePackageVersion("8.0.6.0")),
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("System.ComponentModel.Annotations", CreatePackageVersion("5.0.0.0")),
                        new PackageDependencyInfo("System.Text.Json", CreatePackageVersion("8.0.6.0")),
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("8.0.0.0")),
                        new PackageDependencyInfo("System.Buffers", CreatePackageVersion("4.6.1.0")),
                        new PackageDependencyInfo("System.Memory", CreatePackageVersion("4.6.3.0")),
                        new PackageDependencyInfo("System.Numerics.Vectors", CreatePackageVersion("4.6.1.0")),
                        new PackageDependencyInfo("System.Runtime.CompilerServices.Unsafe", CreatePackageVersion("6.1.2.0")),
                        new PackageDependencyInfo("System.Text.Encodings.Web", CreatePackageVersion("8.0.0.0")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("4.6.3.0")),
                    ])
                )
            },
            ["Aviationexam.Core.Common.SharedInterfaces=0.1.2562.0"] = new(CreatePackageVersion("0.1.2562.0"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Aviationexam.Core.Common.NameOfGenerator.Attributes", CreatePackageVersion("0.1.2562.0")),
                        new PackageDependencyInfo("Microsoft.Extensions.Diagnostics.HealthChecks.Abstractions", CreatePackageVersion("8.0.22.0")),
                        new PackageDependencyInfo("System.ComponentModel.Annotations", CreatePackageVersion("5.0.0.0")),
                        new PackageDependencyInfo("System.Memory", CreatePackageVersion("4.6.3.0")),
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Aviationexam.Core.Common.NameOfGenerator.Attributes", CreatePackageVersion("0.1.2562.0")),
                        new PackageDependencyInfo("Microsoft.Extensions.Diagnostics.HealthChecks.Abstractions", CreatePackageVersion("8.0.22.0")),
                        new PackageDependencyInfo("System.ComponentModel.Annotations", CreatePackageVersion("5.0.0.0")),
                        new PackageDependencyInfo("System.Memory", CreatePackageVersion("4.6.3.0")),
                        new PackageDependencyInfo("System.Buffers", CreatePackageVersion("4.6.1.0")),
                        new PackageDependencyInfo("System.Numerics.Vectors", CreatePackageVersion("4.6.1.0")),
                        new PackageDependencyInfo("System.Runtime.CompilerServices.Unsafe", CreatePackageVersion("6.1.2.0")),
                    ])
                )
            },
            ["Aviationexam.Core.Common.Validation=0.1.2562.0"] = new(CreatePackageVersion("0.1.2562.0"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net8.0", []),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("System.ComponentModel.Annotations", CreatePackageVersion("5.0.0.0")),
                    ])
                )
            },
            ["Microsoft.Bcl.AsyncInterfaces=8.0.0.0"] = null,
            ["Microsoft.Bcl.TimeProvider=8.0.1.0"] = new(CreatePackageVersion("8.0.1.0"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net8.0", []),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("8.0.0.0")),
                    ])
                )
            },
            ["Microsoft.CSharp=4.7.0.0"] = null,
            ["Microsoft.Extensions.DependencyInjection.Abstractions=8.0.2.0"] = null,
            ["Microsoft.Extensions.Diagnostics.HealthChecks.Abstractions=8.0.22.0"] = null,
            ["Microsoft.Extensions.Logging.Abstractions=8.0.3.0"] = null,
            ["Microsoft.IdentityModel.Abstractions=8.15.0.0"] = new(CreatePackageVersion("8.15.0.0"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net8.0", []),
                    new DependencySet("netstandard2.0", [])
                )
            },
            ["Microsoft.IdentityModel.JsonWebTokens=8.15.0.0"] = null,
            ["Microsoft.IdentityModel.Logging=8.15.0.0"] = null,
            ["Microsoft.IdentityModel.Tokens=8.15.0.0"] = null,
            ["System.Buffers=4.5.1.0"] = null,
            ["System.Buffers=4.6.1.0"] = null,
            ["System.Collections.Immutable=8.0.0.0"] = new(CreatePackageVersion("8.0.0.0"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net8.0", []),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("System.Memory", CreatePackageVersion("4.5.5.0")),
                        new PackageDependencyInfo("System.Runtime.CompilerServices.Unsafe", CreatePackageVersion("6.0.0.0")),
                    ])
                )
            },
            ["System.ComponentModel.Annotations=5.0.0.0"] = null,
            ["System.Diagnostics.DiagnosticSource=8.0.1.0"] = null,
            ["System.Memory=4.5.5.0"] = new(CreatePackageVersion("4.5.5.0"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net8.0", []),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("System.Buffers", CreatePackageVersion("4.5.1.0")),
                        new PackageDependencyInfo("System.Numerics.Vectors", CreatePackageVersion("4.4.0.0")),
                        new PackageDependencyInfo("System.Runtime.CompilerServices.Unsafe", CreatePackageVersion("4.5.3.0")),
                    ])
                )
            },
            ["System.Memory=4.6.3.0"] = null,
            ["System.Numerics.Vectors=4.4.0.0"] = null,
            ["System.Numerics.Vectors=4.6.1.0"] = null,
            ["System.Runtime.CompilerServices.Unsafe=4.5.3.0"] = null,
            ["System.Runtime.CompilerServices.Unsafe=6.0.0.0"] = new(CreatePackageVersion("6.0.0.0"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net8.0", []),
                    new DependencySet("netstandard2.0", [])
                )
            },
            ["System.Runtime.CompilerServices.Unsafe=6.1.2.0"] = null,
            ["System.Security.Cryptography.Cng=5.0.0.0"] = new(CreatePackageVersion("5.0.0.0"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net8.0", []),
                    new DependencySet("netstandard2.0", [])
                )
            },
            ["System.Text.Encodings.Web=8.0.0.0"] = null,
            ["System.Text.Json=8.0.6.0"] = null,
            ["System.Threading.Tasks.Extensions=4.6.3.0"] = null,
        }
    )
)
{
    private static readonly NugetTargetFramework Net48 = new("net48");

    private static PackageVersion CreatePackageVersion(
        string version
    ) => new(new Version(version), false, [], NugetReleaseLabelComparer.Instance);

    private static IReadOnlyDictionary<EPackageSource, IReadOnlyCollection<DependencySet>> CreateDependencySets(
        params IReadOnlyCollection<DependencySet> dependencySets
    ) => new Dictionary<EPackageSource, IReadOnlyCollection<DependencySet>>
    {
        [EPackageSource.Default] = dependencySets,
    };

    private static IDictionary<Package, IDictionary<NugetTargetFramework, EDependencyFlag>> CreatePackageFlags(
        params IReadOnlyCollection<(
            Package, NugetTargetFramework, EDependencyFlag
            )> flags
    )
    {
        var packageFlags = new Dictionary<Package, IDictionary<NugetTargetFramework, EDependencyFlag>>();

        foreach (var (package, targetFramework, dependencyFlag) in flags)
        {
            packageFlags.Add(package, new Dictionary<NugetTargetFramework, EDependencyFlag>
            {
                [targetFramework] = dependencyFlag,
            });
        }

        return packageFlags;
    }
}
