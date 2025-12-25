using Aviationexam.DependencyUpdater.Common;
using Aviationexam.DependencyUpdater.Nuget.Extensions;
using Aviationexam.DependencyUpdater.Nuget.Models;
using Aviationexam.DependencyUpdater.Nuget.Services;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using Xunit;
using ZLinq;

namespace Aviationexam.DependencyUpdater.Nuget.Tests;

public sealed partial class FutureDependenciesClassData() : TheoryData<
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
                        ),
                    },
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
                        ),
                    },
                ]
            ),
        ],
        new DependencyProcessingResult(
            CreatePackageFlags(
                (new Package("Aviationexam.Core.Common.JsonInfrastructure", CreatePackageVersion("0.1.2558")),
                    TfF((Net48, EDependencyFlag.Valid))),
                (new Package("Aviationexam.Core.Common.SharedDTOs", CreatePackageVersion("0.1.2562")),
                    TfF((Net48, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("8.0.0")),
                    TfF((Net48, EDependencyFlag.Unknown))),
                (new Package("System.Buffers", CreatePackageVersion("4.6.1")),
                    TfF((Net48, EDependencyFlag.Unknown))),
                (new Package("System.ComponentModel.Annotations", CreatePackageVersion("5.0.0")),
                    TfF((Net48, EDependencyFlag.Unknown))),
                (new Package("System.Memory", CreatePackageVersion("4.6.3")),
                    TfF((Net48, EDependencyFlag.Unknown))),
                (new Package("System.Numerics.Vectors", CreatePackageVersion("4.6.1")),
                    TfF((Net48, EDependencyFlag.Unknown))),
                (new Package("System.Runtime.CompilerServices.Unsafe", CreatePackageVersion("6.1.2")),
                    TfF((Net48, EDependencyFlag.Unknown))),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("8.0.0")),
                    TfF((Net48, EDependencyFlag.Unknown))),
                (new Package("System.Text.Json", CreatePackageVersion("8.0.6")),
                    TfF((Net48, EDependencyFlag.Unknown))),
                (new Package("System.Threading.Tasks.Extensions", CreatePackageVersion("4.6.3")),
                    TfF((Net48, EDependencyFlag.Unknown))),
                (new Package("Aviationexam.Core.Common.Model", CreatePackageVersion("0.1.2558")),
                    TfF((Net48, EDependencyFlag.Valid))),
                (new Package("Aviationexam.Core.Common.SharedInterfaces", CreatePackageVersion("0.1.2562")),
                    TfF((Net48, EDependencyFlag.Unknown))),
                (new Package("Aviationexam.Core.Common.Validation", CreatePackageVersion("0.1.2562")),
                    TfF((Net48, EDependencyFlag.Unknown))),
                (new Package("Microsoft.IdentityModel.JsonWebTokens", CreatePackageVersion("8.15.0")),
                    TfF((Net48, EDependencyFlag.Unknown))),
                (new Package("System.Collections.Immutable", CreatePackageVersion("8.0.0")),
                    TfF((Net48, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Bcl.TimeProvider", CreatePackageVersion("8.0.1")),
                    TfF((Net48, EDependencyFlag.Unknown))),
                (new Package("Microsoft.CSharp", CreatePackageVersion("4.7.0")),
                    TfF((Net48, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("8.0.2")),
                    TfF((Net48, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Diagnostics.HealthChecks.Abstractions", CreatePackageVersion("8.0.22")),
                    TfF((Net48, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("8.0.3")),
                    TfF((Net48, EDependencyFlag.Unknown))),
                (new Package("Microsoft.IdentityModel.Abstractions", CreatePackageVersion("8.15.0")),
                    TfF((Net48, EDependencyFlag.Unknown))),
                (new Package("Microsoft.IdentityModel.Logging", CreatePackageVersion("8.15.0")),
                    TfF((Net48, EDependencyFlag.Unknown))),
                (new Package("Microsoft.IdentityModel.Tokens", CreatePackageVersion("8.15.0")),
                    TfF((Net48, EDependencyFlag.Unknown))),
                (new Package("System.Diagnostics.DiagnosticSource", CreatePackageVersion("8.0.1")),
                    TfF((Net48, EDependencyFlag.Unknown))),
                (new Package("System.Security.Cryptography.Cng", CreatePackageVersion("5.0.0")),
                    TfF((Net48, EDependencyFlag.Unknown)))
            ),
            new Queue<(Package, IReadOnlyCollection<NugetTargetFramework>)>([
                (new Package("Aviationexam.Core.Common.SharedDTOs", CreatePackageVersion("0.1.2562")), [Net48]),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("8.0.0")), [Net48]),
                (new Package("System.Buffers", CreatePackageVersion("4.6.1")), [Net48]),
                (new Package("System.ComponentModel.Annotations", CreatePackageVersion("5.0.0")), [Net48]),
                (new Package("System.Memory", CreatePackageVersion("4.6.3")), [Net48]),
                (new Package("System.Numerics.Vectors", CreatePackageVersion("4.6.1")), [Net48]),
                (new Package("System.Runtime.CompilerServices.Unsafe", CreatePackageVersion("6.1.2")), [Net48]),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("8.0.0")), [Net48]),
                (new Package("System.Text.Json", CreatePackageVersion("8.0.6")), [Net48]),
                (new Package("System.Threading.Tasks.Extensions", CreatePackageVersion("4.6.3")), [Net48]),
                (new Package("Aviationexam.Core.Common.SharedInterfaces", CreatePackageVersion("0.1.2562")), [Net48]),
                (new Package("Aviationexam.Core.Common.Validation", CreatePackageVersion("0.1.2562")), [Net48]),
                (new Package("Microsoft.IdentityModel.JsonWebTokens", CreatePackageVersion("8.15.0")), [Net48]),
                (new Package("System.Collections.Immutable", CreatePackageVersion("8.0.0")), [Net48]),
                (new Package("Microsoft.Bcl.TimeProvider", CreatePackageVersion("8.0.1")), [Net48]),
                (new Package("Microsoft.CSharp", CreatePackageVersion("4.7.0")), [Net48]),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("8.0.2")), [Net48]),
                (new Package("Microsoft.Extensions.Diagnostics.HealthChecks.Abstractions", CreatePackageVersion("8.0.22")), [Net48]),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("8.0.3")), [Net48]),
                (new Package("Microsoft.IdentityModel.Abstractions", CreatePackageVersion("8.15.0")), [Net48]),
                (new Package("Microsoft.IdentityModel.Logging", CreatePackageVersion("8.15.0")), [Net48]),
                (new Package("Microsoft.IdentityModel.Tokens", CreatePackageVersion("8.15.0")), [Net48]),
                (new Package("System.Diagnostics.DiagnosticSource", CreatePackageVersion("8.0.1")), [Net48]),
                (new Package("System.Security.Cryptography.Cng", CreatePackageVersion("5.0.0")), [Net48]),
            ])
        ),
        new Dictionary<string, PackageVersionWithDependencySets?>
        {
            ["Aviationexam.Core.Common.NameOfGenerator.Attributes=0.1.2562.0"] = new(CreatePackageVersion("0.1.2562.0"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("netstandard2.0", [])
                ),
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
                ),
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
                ),
            },
            ["Aviationexam.Core.Common.Validation=0.1.2562.0"] = new(CreatePackageVersion("0.1.2562.0"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net8.0", []),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("System.ComponentModel.Annotations", CreatePackageVersion("5.0.0.0")),
                    ])
                ),
            },
            ["Microsoft.Bcl.AsyncInterfaces=8.0.0.0"] = null,
            ["Microsoft.Bcl.TimeProvider=8.0.1.0"] = new(CreatePackageVersion("8.0.1.0"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net8.0", []),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("8.0.0.0")),
                    ])
                ),
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
                ),
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
                ),
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
                ),
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
                ),
            },
            ["System.Runtime.CompilerServices.Unsafe=6.1.2.0"] = null,
            ["System.Security.Cryptography.Cng=5.0.0.0"] = new(CreatePackageVersion("5.0.0.0"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net8.0", []),
                    new DependencySet("netstandard2.0", [])
                ),
            },
            ["System.Text.Encodings.Web=8.0.0.0"] = null,
            ["System.Text.Json=8.0.6.0"] = null,
            ["System.Threading.Tasks.Extensions=4.6.3.0"] = null,
        }
    ),
    (
        [
            KeyValuePair.Create(
                new NugetDependency(
                    new NugetFile("", ENugetFileType.Csproj),
                    new NugetPackageReference("Castle.Core", VersionRange.Parse("5.2.1")),
                    [Net100]
                ),
                FactoryClass_e9e439747fbd44678abb709f9dc169a2.CreateDependencySetsCastle_Core()
            ),
            KeyValuePair.Create(
                new NugetDependency(
                    new NugetFile("", ENugetFileType.Csproj),
                    new NugetPackageReference("Meziantou.Analyzer", VersionRange.Parse("2.0.263")),
                    [Net100, Net80, Net90]
                ),
                (IReadOnlyCollection<PackageVersionWithDependencySets>)
                [
                    new PackageVersionWithDependencySets(CreatePackageVersion("2.0.264"))
                    {
                        DependencySets = CreateDependencySets(
                            new DependencySet("netstandard2.0", [
                            ])
                        ),
                    }
                ]
            ),
            KeyValuePair.Create(
                new NugetDependency(
                    new NugetFile("", ENugetFileType.Csproj),
                    new NugetPackageReference("Meziantou.Extensions.Logging.Xunit.v3", VersionRange.Parse("1.1.19")),
                    [Net80]
                ),
                FactoryClass_e9e439747fbd44678abb709f9dc169a2.CreateDependencySetsMeziantou_Extensions_Logging_Xunit_v3()
            ),
            KeyValuePair.Create(
                new NugetDependency(
                    new NugetFile("", ENugetFileType.Csproj),
                    new NugetPackageReference("Microsoft.AspNetCore.WebUtilities", VersionRange.Parse("8.0.22")),
                    [Net80]
                ),
                FactoryClass_e9e439747fbd44678abb709f9dc169a2.CreateDependencySetsMicrosoft_AspNetCore_WebUtilities()
            ),
            KeyValuePair.Create(
                new NugetDependency(
                    new NugetFile("", ENugetFileType.Csproj),
                    new NugetPackageReference("Microsoft.AspNetCore.WebUtilities", VersionRange.Parse("9.0.11")),
                    [Net90]
                ),
                FactoryClass_e9e439747fbd44678abb709f9dc169a2.CreateDependencySetsMicrosoft_AspNetCore_WebUtilities()
            ),
            KeyValuePair.Create(
                new NugetDependency(
                    new NugetFile("", ENugetFileType.Csproj),
                    new NugetPackageReference("Microsoft.AspNetCore.WebUtilities", VersionRange.Parse("10.0.1")),
                    [Net100]
                ),
                FactoryClass_e9e439747fbd44678abb709f9dc169a2.CreateDependencySetsMicrosoft_AspNetCore_WebUtilities()
            ),
            KeyValuePair.Create(
                new NugetDependency(
                    new NugetFile("", ENugetFileType.Csproj),
                    new NugetPackageReference("Microsoft.Bcl.AsyncInterfaces", VersionRange.Parse("10.0.1")),
                    [Net100]
                ),
                FactoryClass_e9e439747fbd44678abb709f9dc169a2.CreateDependencySetsMicrosoft_Bcl_AsyncInterfaces()
            ),
            KeyValuePair.Create(
                new NugetDependency(
                    new NugetFile("", ENugetFileType.Csproj),
                    new NugetPackageReference("Microsoft.Bcl.AsyncInterfaces", VersionRange.Parse("9.0.11")),
                    [Net90]
                ),
                FactoryClass_e9e439747fbd44678abb709f9dc169a2.CreateDependencySetsMicrosoft_Bcl_AsyncInterfaces()
            ),
            KeyValuePair.Create(
                new NugetDependency(
                    new NugetFile("", ENugetFileType.Csproj),
                    new NugetPackageReference("Microsoft.Extensions.Caching.Abstractions", VersionRange.Parse("8.0.0")),
                    [Net80]
                ),
                FactoryClass_e9e439747fbd44678abb709f9dc169a2.CreateDependencySetsMicrosoft_Extensions_Caching_Abstractions()
            ),
            KeyValuePair.Create(
                new NugetDependency(
                    new NugetFile("", ENugetFileType.Csproj),
                    new NugetPackageReference("Microsoft.Extensions.Caching.Abstractions", VersionRange.Parse("10.0.1")),
                    [Net100]
                ),
                FactoryClass_e9e439747fbd44678abb709f9dc169a2.CreateDependencySetsMicrosoft_Extensions_Caching_Abstractions()
            ),
            KeyValuePair.Create(
                new NugetDependency(
                    new NugetFile("", ENugetFileType.Csproj),
                    new NugetPackageReference("Microsoft.Extensions.Caching.Abstractions", VersionRange.Parse("9.0.11")),
                    [Net90]
                ),
                FactoryClass_e9e439747fbd44678abb709f9dc169a2.CreateDependencySetsMicrosoft_Extensions_Caching_Abstractions()
            ),
            KeyValuePair.Create(
                new NugetDependency(
                    new NugetFile("", ENugetFileType.Csproj),
                    new NugetPackageReference("Microsoft.Extensions.DependencyInjection", VersionRange.Parse("8.0.1")),
                    [Net80]
                ),
                FactoryClass_e9e439747fbd44678abb709f9dc169a2.CreateDependencySetsMicrosoft_Extensions_DependencyInjection()
            ),
            KeyValuePair.Create(
                new NugetDependency(
                    new NugetFile("", ENugetFileType.Csproj),
                    new NugetPackageReference("Microsoft.Extensions.DependencyInjection", VersionRange.Parse("9.0.11")),
                    [Net90]
                ),
                FactoryClass_e9e439747fbd44678abb709f9dc169a2.CreateDependencySetsMicrosoft_Extensions_DependencyInjection()
            ),
            KeyValuePair.Create(
                new NugetDependency(
                    new NugetFile("", ENugetFileType.Csproj),
                    new NugetPackageReference("Microsoft.Extensions.DependencyInjection", VersionRange.Parse("10.0.1")),
                    [Net100]
                ),
                FactoryClass_e9e439747fbd44678abb709f9dc169a2.CreateDependencySetsMicrosoft_Extensions_DependencyInjection()
            ),
            KeyValuePair.Create(
                new NugetDependency(
                    new NugetFile("", ENugetFileType.Csproj),
                    new NugetPackageReference("Microsoft.Extensions.DependencyInjection.Abstractions", VersionRange.Parse("10.0.1")),
                    [Net100]
                ),
                FactoryClass_e9e439747fbd44678abb709f9dc169a2.CreateDependencySetsMicrosoft_Extensions_DependencyInjection_Abstractions()
            ),
            KeyValuePair.Create(
                new NugetDependency(
                    new NugetFile("", ENugetFileType.Csproj),
                    new NugetPackageReference("Microsoft.Extensions.DependencyInjection.Abstractions", VersionRange.Parse("9.0.11")),
                    [Net90]
                ),
                FactoryClass_e9e439747fbd44678abb709f9dc169a2.CreateDependencySetsMicrosoft_Extensions_DependencyInjection_Abstractions()
            ),
            KeyValuePair.Create(
                new NugetDependency(
                    new NugetFile("", ENugetFileType.Csproj),
                    new NugetPackageReference("Microsoft.Extensions.DependencyInjection.Abstractions", VersionRange.Parse("8.0.2")),
                    [Net80]
                ),
                FactoryClass_e9e439747fbd44678abb709f9dc169a2.CreateDependencySetsMicrosoft_Extensions_DependencyInjection_Abstractions()
            ),
            KeyValuePair.Create(
                new NugetDependency(
                    new NugetFile("", ENugetFileType.Csproj),
                    new NugetPackageReference("Microsoft.Extensions.Http", VersionRange.Parse("8.0.1")),
                    [Net80]
                ),
                FactoryClass_e9e439747fbd44678abb709f9dc169a2.CreateDependencySetsMicrosoft_Extensions_Http()
            ),
            KeyValuePair.Create(
                new NugetDependency(
                    new NugetFile("", ENugetFileType.Csproj),
                    new NugetPackageReference("Microsoft.Extensions.Http", VersionRange.Parse("10.0.1")),
                    [Net100]
                ),
                FactoryClass_e9e439747fbd44678abb709f9dc169a2.CreateDependencySetsMicrosoft_Extensions_Http()
            ),
            KeyValuePair.Create(
                new NugetDependency(
                    new NugetFile("", ENugetFileType.Csproj),
                    new NugetPackageReference("Microsoft.Extensions.Http", VersionRange.Parse("9.0.11")),
                    [Net90]
                ),
                FactoryClass_e9e439747fbd44678abb709f9dc169a2.CreateDependencySetsMicrosoft_Extensions_Http()
            ),
            KeyValuePair.Create(
                new NugetDependency(
                    new NugetFile("", ENugetFileType.Csproj),
                    new NugetPackageReference("Microsoft.Extensions.Options", VersionRange.Parse("8.0.2")),
                    [Net80]
                ),
                FactoryClass_e9e439747fbd44678abb709f9dc169a2.CreateDependencySetsMicrosoft_Extensions_Options()
            ),
            KeyValuePair.Create(
                new NugetDependency(
                    new NugetFile("", ENugetFileType.Csproj),
                    new NugetPackageReference("Microsoft.Extensions.Options", VersionRange.Parse("9.0.11")),
                    [Net90]
                ),
                FactoryClass_e9e439747fbd44678abb709f9dc169a2.CreateDependencySetsMicrosoft_Extensions_Options()
            ),
            KeyValuePair.Create(
                new NugetDependency(
                    new NugetFile("", ENugetFileType.Csproj),
                    new NugetPackageReference("Microsoft.Extensions.Options", VersionRange.Parse("10.0.1")),
                    [Net100]
                ),
                FactoryClass_e9e439747fbd44678abb709f9dc169a2.CreateDependencySetsMicrosoft_Extensions_Options()
            ),
            KeyValuePair.Create(
                new NugetDependency(
                    new NugetFile("", ENugetFileType.Csproj),
                    new NugetPackageReference("Microsoft.Kiota.Abstractions", VersionRange.Parse("1.21.0")),
                    [Net80, Net90, Net100]
                ),
                (IReadOnlyCollection<PackageVersionWithDependencySets>)
                [
                    new PackageVersionWithDependencySets(CreatePackageVersion("1.21.1"))
                    {
                        DependencySets = CreateDependencySets(
                            new DependencySet("net5.0", [
                                new PackageDependencyInfo("Std.UriTemplate", CreatePackageVersion("2.0.8")),
                                new PackageDependencyInfo("System.Diagnostics.DiagnosticSource", CreatePackageVersion("6.0.0"))
                            ]),
                            new DependencySet("net6.0", [
                                new PackageDependencyInfo("Std.UriTemplate", CreatePackageVersion("2.0.8"))
                            ]),
                            new DependencySet("net8.0", [
                                new PackageDependencyInfo("Std.UriTemplate", CreatePackageVersion("2.0.8"))
                            ]),
                            new DependencySet("netstandard2.0", [
                                new PackageDependencyInfo("Std.UriTemplate", CreatePackageVersion("2.0.8")),
                                new PackageDependencyInfo("System.Diagnostics.DiagnosticSource", CreatePackageVersion("6.0.0"))
                            ]),
                            new DependencySet("netstandard2.1", [
                                new PackageDependencyInfo("Std.UriTemplate", CreatePackageVersion("2.0.8")),
                                new PackageDependencyInfo("System.Diagnostics.DiagnosticSource", CreatePackageVersion("6.0.0"))
                            ])
                        ),
                    }
                ]
            ),
            KeyValuePair.Create(
                new NugetDependency(
                    new NugetFile("", ENugetFileType.Csproj),
                    new NugetPackageReference("Microsoft.Kiota.Http.HttpClientLibrary", VersionRange.Parse("1.21.0")),
                    [Net80, Net90, Net100]
                ),
                (IReadOnlyCollection<PackageVersionWithDependencySets>)
                [
                    new PackageVersionWithDependencySets(CreatePackageVersion("1.21.1"))
                    {
                        DependencySets = CreateDependencySets(
                            new DependencySet("net462", [
                                new PackageDependencyInfo("Microsoft.Kiota.Abstractions", CreatePackageVersion("1.21.1")),
                                new PackageDependencyInfo("System.Diagnostics.DiagnosticSource", CreatePackageVersion("6.0.0")),
                                new PackageDependencyInfo("System.Net.Http.WinHttpHandler", CreatePackageVersion("6.0.0")),
                                new PackageDependencyInfo("System.Text.Json", CreatePackageVersion("6.0.10"))
                            ]),
                            new DependencySet("net5.0", [
                                new PackageDependencyInfo("Microsoft.Kiota.Abstractions", CreatePackageVersion("1.21.1")),
                                new PackageDependencyInfo("System.Diagnostics.DiagnosticSource", CreatePackageVersion("6.0.0")),
                                new PackageDependencyInfo("System.Text.Json", CreatePackageVersion("6.0.10"))
                            ]),
                            new DependencySet("net6.0", [
                                new PackageDependencyInfo("Microsoft.Kiota.Abstractions", CreatePackageVersion("1.21.1"))
                            ]),
                            new DependencySet("net8.0", [
                                new PackageDependencyInfo("Microsoft.Kiota.Abstractions", CreatePackageVersion("1.21.1"))
                            ]),
                            new DependencySet("net8.0-browser1.0", [
                                new PackageDependencyInfo("Microsoft.Kiota.Abstractions", CreatePackageVersion("1.21.1"))
                            ]),
                            new DependencySet("netstandard2.0", [
                                new PackageDependencyInfo("Microsoft.Kiota.Abstractions", CreatePackageVersion("1.21.1")),
                                new PackageDependencyInfo("System.Diagnostics.DiagnosticSource", CreatePackageVersion("6.0.0")),
                                new PackageDependencyInfo("System.Text.Json", CreatePackageVersion("6.0.10"))
                            ]),
                            new DependencySet("netstandard2.1", [
                                new PackageDependencyInfo("Microsoft.Kiota.Abstractions", CreatePackageVersion("1.21.1")),
                                new PackageDependencyInfo("System.Diagnostics.DiagnosticSource", CreatePackageVersion("6.0.0")),
                                new PackageDependencyInfo("System.Text.Json", CreatePackageVersion("6.0.10"))
                            ])
                        ),
                    }
                ]
            ),
            KeyValuePair.Create(
                new NugetDependency(
                    new NugetFile("", ENugetFileType.Csproj),
                    new NugetPackageReference("Microsoft.Kiota.Serialization.Form", VersionRange.Parse("1.21.0")),
                    [Net80, Net90, Net100]
                ),
                (IReadOnlyCollection<PackageVersionWithDependencySets>)
                [
                    new PackageVersionWithDependencySets(CreatePackageVersion("1.21.1"))
                    {
                        DependencySets = CreateDependencySets(
                            new DependencySet("net5.0", [
                                new PackageDependencyInfo("Microsoft.Kiota.Abstractions", CreatePackageVersion("1.21.1"))
                            ]),
                            new DependencySet("net6.0", [
                                new PackageDependencyInfo("Microsoft.Kiota.Abstractions", CreatePackageVersion("1.21.1"))
                            ]),
                            new DependencySet("net8.0", [
                                new PackageDependencyInfo("Microsoft.Kiota.Abstractions", CreatePackageVersion("1.21.1"))
                            ]),
                            new DependencySet("netstandard2.0", [
                                new PackageDependencyInfo("Microsoft.Kiota.Abstractions", CreatePackageVersion("1.21.1"))
                            ]),
                            new DependencySet("netstandard2.1", [
                                new PackageDependencyInfo("Microsoft.Kiota.Abstractions", CreatePackageVersion("1.21.1"))
                            ])
                        ),
                    }
                ]
            ),
            KeyValuePair.Create(
                new NugetDependency(
                    new NugetFile("", ENugetFileType.Csproj),
                    new NugetPackageReference("Microsoft.Kiota.Serialization.Json", VersionRange.Parse("1.21.0")),
                    [Net80, Net90, Net100]
                ),
                (IReadOnlyCollection<PackageVersionWithDependencySets>)
                [
                    new PackageVersionWithDependencySets(CreatePackageVersion("1.21.1"))
                    {
                        DependencySets = CreateDependencySets(
                            new DependencySet("net5.0", [
                                new PackageDependencyInfo("Microsoft.Kiota.Abstractions", CreatePackageVersion("1.21.1")),
                                new PackageDependencyInfo("System.Text.Json", CreatePackageVersion("6.0.10"))
                            ]),
                            new DependencySet("net6.0", [
                                new PackageDependencyInfo("Microsoft.Kiota.Abstractions", CreatePackageVersion("1.21.1"))
                            ]),
                            new DependencySet("net8.0", [
                                new PackageDependencyInfo("Microsoft.Kiota.Abstractions", CreatePackageVersion("1.21.1"))
                            ]),
                            new DependencySet("netstandard2.0", [
                                new PackageDependencyInfo("Microsoft.Kiota.Abstractions", CreatePackageVersion("1.21.1")),
                                new PackageDependencyInfo("System.Text.Json", CreatePackageVersion("6.0.10"))
                            ]),
                            new DependencySet("netstandard2.1", [
                                new PackageDependencyInfo("Microsoft.Kiota.Abstractions", CreatePackageVersion("1.21.1")),
                                new PackageDependencyInfo("System.Text.Json", CreatePackageVersion("6.0.10"))
                            ])
                        ),
                    }
                ]
            ),
            KeyValuePair.Create(
                new NugetDependency(
                    new NugetFile("", ENugetFileType.Csproj),
                    new NugetPackageReference("Microsoft.Kiota.Serialization.Multipart", VersionRange.Parse("1.21.0")),
                    [Net80, Net90, Net100]
                ),
                (IReadOnlyCollection<PackageVersionWithDependencySets>)
                [
                    new PackageVersionWithDependencySets(CreatePackageVersion("1.21.1"))
                    {
                        DependencySets = CreateDependencySets(
                            new DependencySet("net5.0", [
                                new PackageDependencyInfo("Microsoft.Kiota.Abstractions", CreatePackageVersion("1.21.1"))
                            ]),
                            new DependencySet("net6.0", [
                                new PackageDependencyInfo("Microsoft.Kiota.Abstractions", CreatePackageVersion("1.21.1"))
                            ]),
                            new DependencySet("net8.0", [
                                new PackageDependencyInfo("Microsoft.Kiota.Abstractions", CreatePackageVersion("1.21.1"))
                            ]),
                            new DependencySet("netstandard2.0", [
                                new PackageDependencyInfo("Microsoft.Kiota.Abstractions", CreatePackageVersion("1.21.1"))
                            ]),
                            new DependencySet("netstandard2.1", [
                                new PackageDependencyInfo("Microsoft.Kiota.Abstractions", CreatePackageVersion("1.21.1"))
                            ])
                        ),
                    }
                ]
            ),
            KeyValuePair.Create(
                new NugetDependency(
                    new NugetFile("", ENugetFileType.Csproj),
                    new NugetPackageReference("Microsoft.Kiota.Serialization.Text", VersionRange.Parse("1.21.0")),
                    [Net80, Net90, Net100]
                ),
                (IReadOnlyCollection<PackageVersionWithDependencySets>)
                [
                    new PackageVersionWithDependencySets(CreatePackageVersion("1.21.1"))
                    {
                        DependencySets = CreateDependencySets(
                            new DependencySet("net5.0", [
                                new PackageDependencyInfo("Microsoft.Kiota.Abstractions", CreatePackageVersion("1.21.1"))
                            ]),
                            new DependencySet("net6.0", [
                                new PackageDependencyInfo("Microsoft.Kiota.Abstractions", CreatePackageVersion("1.21.1"))
                            ]),
                            new DependencySet("net8.0", [
                                new PackageDependencyInfo("Microsoft.Kiota.Abstractions", CreatePackageVersion("1.21.1"))
                            ]),
                            new DependencySet("netstandard2.0", [
                                new PackageDependencyInfo("Microsoft.Kiota.Abstractions", CreatePackageVersion("1.21.1"))
                            ]),
                            new DependencySet("netstandard2.1", [
                                new PackageDependencyInfo("Microsoft.Kiota.Abstractions", CreatePackageVersion("1.21.1"))
                            ])
                        ),
                    }
                ]
            ),
            KeyValuePair.Create(
                new NugetDependency(
                    new NugetFile("", ENugetFileType.Csproj),
                    new NugetPackageReference("Microsoft.Testing.Extensions.CodeCoverage", VersionRange.Parse("18.1.0")),
                    [Net100]
                ),
                FactoryClass_e9e439747fbd44678abb709f9dc169a2.CreateDependencySetsMicrosoft_Testing_Extensions_CodeCoverage()
            ),
            KeyValuePair.Create(
                new NugetDependency(
                    new NugetFile("", ENugetFileType.Csproj),
                    new NugetPackageReference("Microsoft.Testing.Extensions.TrxReport", VersionRange.Parse("2.0.2")),
                    [Net100]
                ),
                FactoryClass_e9e439747fbd44678abb709f9dc169a2.CreateDependencySetsMicrosoft_Testing_Extensions_TrxReport()
            ),
            KeyValuePair.Create(
                new NugetDependency(
                    new NugetFile("", ENugetFileType.Csproj),
                    new NugetPackageReference("NSubstitute", VersionRange.Parse("5.3.0")),
                    [Net100]
                ),
                FactoryClass_e9e439747fbd44678abb709f9dc169a2.CreateDependencySetsNSubstitute()
            ),
            KeyValuePair.Create(
                new NugetDependency(
                    new NugetFile("", ENugetFileType.Csproj),
                    new NugetPackageReference("NSubstitute.Analyzers.CSharp", VersionRange.Parse("1.0.17")),
                    [Net100]
                ),
                FactoryClass_e9e439747fbd44678abb709f9dc169a2.CreateDependencySetsNSubstitute_Analyzers_CSharp()
            ),
            KeyValuePair.Create(
                new NugetDependency(
                    new NugetFile("", ENugetFileType.Csproj),
                    new NugetPackageReference("System.Text.Json", VersionRange.Parse("10.0.1")),
                    [Net100]
                ),
                FactoryClass_e9e439747fbd44678abb709f9dc169a2.CreateDependencySetsSystem_Text_Json()
            ),
            KeyValuePair.Create(
                new NugetDependency(
                    new NugetFile("", ENugetFileType.Csproj),
                    new NugetPackageReference("System.Text.Json", VersionRange.Parse("8.0.6")),
                    [Net80]
                ),
                FactoryClass_e9e439747fbd44678abb709f9dc169a2.CreateDependencySetsSystem_Text_Json()
            ),
            KeyValuePair.Create(
                new NugetDependency(
                    new NugetFile("", ENugetFileType.Csproj),
                    new NugetPackageReference("System.Text.Json", VersionRange.Parse("9.0.11")),
                    [Net90]
                ),
                FactoryClass_e9e439747fbd44678abb709f9dc169a2.CreateDependencySetsSystem_Text_Json()
            ),
            KeyValuePair.Create(
                new NugetDependency(
                    new NugetFile("", ENugetFileType.Csproj),
                    new NugetPackageReference("ZLinq", VersionRange.Parse("1.5.4")),
                    [Net100, Net80, Net90]
                ),
                (IReadOnlyCollection<PackageVersionWithDependencySets>)
                [
                ]
            ),
            KeyValuePair.Create(
                new NugetDependency(
                    new NugetFile("", ENugetFileType.Csproj),
                    new NugetPackageReference("ZeroQL", VersionRange.Parse("8.0.0-preview.7")),
                    [Net80, Net90, Net100]
                ),
                (IReadOnlyCollection<PackageVersionWithDependencySets>)
                [
                    new PackageVersionWithDependencySets(CreatePackageVersion("8.0.0-preview.8"))
                    {
                        DependencySets = CreateDependencySets(
                        ),
                    },
                    new PackageVersionWithDependencySets(CreatePackageVersion("8.0.0"))
                    {
                        DependencySets = CreateDependencySets(
                        ),
                    }
                ]
            ),
            KeyValuePair.Create(
                new NugetDependency(
                    new NugetFile("", ENugetFileType.Csproj),
                    new NugetPackageReference("microsoft.openapi.kiota", VersionRange.Parse("1.29.0")),
                    [Net100]
                ),
                FactoryClass_e9e439747fbd44678abb709f9dc169a2.CreateDependencySetsmicrosoft_openapi_kiota()
            ),
            KeyValuePair.Create(
                new NugetDependency(
                    new NugetFile("", ENugetFileType.Csproj),
                    new NugetPackageReference("xunit.v3.mtp-v2", VersionRange.Parse("3.2.1")),
                    [Net100]
                ),
                FactoryClass_e9e439747fbd44678abb709f9dc169a2.CreateDependencySetsxunit_v3_mtp_v2()
            ),
            KeyValuePair.Create(
                new NugetDependency(
                    new NugetFile("", ENugetFileType.Csproj),
                    new NugetPackageReference("zeroql.cli", VersionRange.Parse("8.0.0-preview.7")),
                    [Net100]
                ),
                FactoryClass_e9e439747fbd44678abb709f9dc169a2.CreateDependencySetszeroql_cli()
            ),
        ],
        new DependencyProcessingResult(
            CreatePackageFlags(
                (new Package("Castle.Core", CreatePackageVersion("5.2.1")),
                    TfF((Net100, EDependencyFlag.Valid))),
                (new Package("Meziantou.Analyzer", CreatePackageVersion("2.0.263")),
                    TfF((Net100, EDependencyFlag.Valid), (Net80, EDependencyFlag.Valid), (Net90, EDependencyFlag.Valid))),
                (new Package("Meziantou.Extensions.Logging.Xunit.v3", CreatePackageVersion("1.1.19")),
                    TfF((Net80, EDependencyFlag.Valid))),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("10.0.1")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("xunit.v3.extensibility.core", CreatePackageVersion("3.2.1")),
                    TfF((Net80, EDependencyFlag.Unknown))),
                (new Package("Microsoft.AspNetCore.WebUtilities", CreatePackageVersion("8.0.22")),
                    TfF((Net80, EDependencyFlag.Valid))),
                (new Package("Microsoft.AspNetCore.WebUtilities", CreatePackageVersion("9.0.11")),
                    TfF((Net90, EDependencyFlag.Valid))),
                (new Package("Microsoft.Net.Http.Headers", CreatePackageVersion("9.0.0")),
                    TfF((Net90, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Net.Http.Headers", CreatePackageVersion("9.0.1")),
                    TfF((Net90, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Net.Http.Headers", CreatePackageVersion("9.0.2")),
                    TfF((Net90, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Net.Http.Headers", CreatePackageVersion("9.0.3")),
                    TfF((Net90, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Net.Http.Headers", CreatePackageVersion("9.0.4")),
                    TfF((Net90, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Net.Http.Headers", CreatePackageVersion("9.0.5")),
                    TfF((Net90, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Net.Http.Headers", CreatePackageVersion("9.0.6")),
                    TfF((Net90, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Net.Http.Headers", CreatePackageVersion("9.0.7")),
                    TfF((Net90, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Net.Http.Headers", CreatePackageVersion("9.0.8")),
                    TfF((Net90, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Net.Http.Headers", CreatePackageVersion("9.0.9")),
                    TfF((Net90, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Net.Http.Headers", CreatePackageVersion("9.0.10")),
                    TfF((Net90, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Net.Http.Headers", CreatePackageVersion("9.0.11")),
                    TfF((Net90, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown))),
                (new Package("Microsoft.AspNetCore.WebUtilities", CreatePackageVersion("10.0.1")),
                    TfF((Net100, EDependencyFlag.Valid))),
                (new Package("Microsoft.Net.Http.Headers", CreatePackageVersion("10.0.0")),
                    TfF((Net100, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Net.Http.Headers", CreatePackageVersion("10.0.1")),
                    TfF((Net100, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("10.0.1")),
                    TfF((Net100, EDependencyFlag.Valid), (Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("System.Threading.Tasks.Extensions", CreatePackageVersion("4.6.3")),
                    TfF((Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown), (Net80, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("9.0.11")),
                    TfF((Net90, EDependencyFlag.Valid), (Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Caching.Abstractions", CreatePackageVersion("8.0.0")),
                    TfF((Net80, EDependencyFlag.Valid))),
                (new Package("Microsoft.Extensions.Primitives", CreatePackageVersion("9.0.0")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("System.Threading.Tasks.Extensions", CreatePackageVersion("4.5.4")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Primitives", CreatePackageVersion("9.0.1")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Primitives", CreatePackageVersion("9.0.2")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Primitives", CreatePackageVersion("9.0.3")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Primitives", CreatePackageVersion("9.0.4")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Primitives", CreatePackageVersion("9.0.5")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Primitives", CreatePackageVersion("9.0.6")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Primitives", CreatePackageVersion("9.0.7")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Primitives", CreatePackageVersion("9.0.8")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Primitives", CreatePackageVersion("9.0.9")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Primitives", CreatePackageVersion("9.0.10")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Primitives", CreatePackageVersion("9.0.11")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Primitives", CreatePackageVersion("10.0.0")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Primitives", CreatePackageVersion("10.0.1")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Caching.Abstractions", CreatePackageVersion("10.0.1")),
                    TfF((Net100, EDependencyFlag.Valid))),
                (new Package("Microsoft.Extensions.Caching.Abstractions", CreatePackageVersion("9.0.11")),
                    TfF((Net90, EDependencyFlag.Valid))),
                (new Package("Microsoft.Extensions.DependencyInjection", CreatePackageVersion("8.0.1")),
                    TfF((Net80, EDependencyFlag.Valid))),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("9.0.0")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("9.0.0")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("9.0.1")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("9.0.1")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("9.0.2")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("9.0.2")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("9.0.3")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("9.0.3")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("9.0.4")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("9.0.4")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("9.0.5")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("9.0.5")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("9.0.6")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("9.0.6")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("9.0.7")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("9.0.7")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("9.0.8")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("9.0.8")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("9.0.9")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("9.0.9")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("9.0.10")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("9.0.10")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("9.0.11")),
                    TfF((Net90, EDependencyFlag.Valid), (Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("10.0.0")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("10.0.0")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("10.0.1")),
                    TfF((Net100, EDependencyFlag.Valid), (Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.DependencyInjection", CreatePackageVersion("9.0.11")),
                    TfF((Net90, EDependencyFlag.Valid))),
                (new Package("Microsoft.Extensions.DependencyInjection", CreatePackageVersion("10.0.1")),
                    TfF((Net100, EDependencyFlag.Valid))),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("8.0.2")),
                    TfF((Net80, EDependencyFlag.Valid))),
                (new Package("Microsoft.Extensions.Http", CreatePackageVersion("8.0.1")),
                    TfF((Net80, EDependencyFlag.Valid))),
                (new Package("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("9.0.0")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Diagnostics", CreatePackageVersion("9.0.0")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("9.0.0")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Logging", CreatePackageVersion("9.0.0")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Options", CreatePackageVersion("9.0.0")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("9.0.1")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Diagnostics", CreatePackageVersion("9.0.1")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("9.0.1")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Logging", CreatePackageVersion("9.0.1")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Options", CreatePackageVersion("9.0.1")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("9.0.2")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Diagnostics", CreatePackageVersion("9.0.2")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("9.0.2")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Logging", CreatePackageVersion("9.0.2")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Options", CreatePackageVersion("9.0.2")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("9.0.3")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Diagnostics", CreatePackageVersion("9.0.3")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("9.0.3")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Logging", CreatePackageVersion("9.0.3")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Options", CreatePackageVersion("9.0.3")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("9.0.4")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Diagnostics", CreatePackageVersion("9.0.4")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("9.0.4")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Logging", CreatePackageVersion("9.0.4")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Options", CreatePackageVersion("9.0.4")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("9.0.5")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Diagnostics", CreatePackageVersion("9.0.5")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("9.0.5")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Logging", CreatePackageVersion("9.0.5")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Options", CreatePackageVersion("9.0.5")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("9.0.6")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Diagnostics", CreatePackageVersion("9.0.6")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("9.0.6")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Logging", CreatePackageVersion("9.0.6")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Options", CreatePackageVersion("9.0.6")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("9.0.7")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Diagnostics", CreatePackageVersion("9.0.7")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("9.0.7")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Logging", CreatePackageVersion("9.0.7")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Options", CreatePackageVersion("9.0.7")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("9.0.8")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Diagnostics", CreatePackageVersion("9.0.8")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("9.0.8")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Logging", CreatePackageVersion("9.0.8")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Options", CreatePackageVersion("9.0.8")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("9.0.9")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Diagnostics", CreatePackageVersion("9.0.9")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("9.0.9")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Logging", CreatePackageVersion("9.0.9")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Options", CreatePackageVersion("9.0.9")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("9.0.10")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Diagnostics", CreatePackageVersion("9.0.10")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("9.0.10")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Logging", CreatePackageVersion("9.0.10")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Options", CreatePackageVersion("9.0.10")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("9.0.11")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Diagnostics", CreatePackageVersion("9.0.11")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("9.0.11")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Logging", CreatePackageVersion("9.0.11")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Options", CreatePackageVersion("9.0.11")),
                    TfF((Net90, EDependencyFlag.Valid))),
                (new Package("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("10.0.0")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Diagnostics", CreatePackageVersion("10.0.0")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("10.0.0")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Logging", CreatePackageVersion("10.0.0")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Options", CreatePackageVersion("10.0.0")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("10.0.1")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Diagnostics", CreatePackageVersion("10.0.1")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Logging", CreatePackageVersion("10.0.1")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Extensions.Options", CreatePackageVersion("10.0.1")),
                    TfF((Net100, EDependencyFlag.Valid))),
                (new Package("Microsoft.Extensions.Http", CreatePackageVersion("10.0.1")),
                    TfF((Net100, EDependencyFlag.Valid))),
                (new Package("Microsoft.Extensions.Http", CreatePackageVersion("9.0.11")),
                    TfF((Net90, EDependencyFlag.Valid))),
                (new Package("Microsoft.Extensions.Options", CreatePackageVersion("8.0.2")),
                    TfF((Net80, EDependencyFlag.Valid))),
                (new Package("System.ComponentModel.Annotations", CreatePackageVersion("5.0.0")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Kiota.Abstractions", CreatePackageVersion("1.21.0")),
                    TfF((Net80, EDependencyFlag.Valid), (Net90, EDependencyFlag.Valid), (Net100, EDependencyFlag.Valid))),
                (new Package("Std.UriTemplate", CreatePackageVersion("2.0.8")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown))),
                (new Package("System.Diagnostics.DiagnosticSource", CreatePackageVersion("6.0.0")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Kiota.Http.HttpClientLibrary", CreatePackageVersion("1.21.0")),
                    TfF((Net80, EDependencyFlag.Valid), (Net90, EDependencyFlag.Valid), (Net100, EDependencyFlag.Valid))),
                (new Package("Microsoft.Kiota.Abstractions", CreatePackageVersion("1.21.1")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown))),
                (new Package("System.Text.Json", CreatePackageVersion("6.0.10")),
                    TfF((Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown), (Net100, EDependencyFlag.Unknown))),
                (new Package("Microsoft.Kiota.Serialization.Form", CreatePackageVersion("1.21.0")),
                    TfF((Net80, EDependencyFlag.Valid), (Net90, EDependencyFlag.Valid), (Net100, EDependencyFlag.Valid))),
                (new Package("Microsoft.Kiota.Serialization.Json", CreatePackageVersion("1.21.0")),
                    TfF((Net80, EDependencyFlag.Valid), (Net90, EDependencyFlag.Valid), (Net100, EDependencyFlag.Valid))),
                (new Package("Microsoft.Kiota.Serialization.Multipart", CreatePackageVersion("1.21.0")),
                    TfF((Net80, EDependencyFlag.Valid), (Net90, EDependencyFlag.Valid), (Net100, EDependencyFlag.Valid))),
                (new Package("Microsoft.Kiota.Serialization.Text", CreatePackageVersion("1.21.0")),
                    TfF((Net80, EDependencyFlag.Valid), (Net90, EDependencyFlag.Valid), (Net100, EDependencyFlag.Valid))),
                (new Package("Microsoft.Testing.Extensions.CodeCoverage", CreatePackageVersion("18.1.0")),
                    TfF((Net100, EDependencyFlag.Valid))),
                (new Package("Microsoft.Testing.Extensions.TrxReport", CreatePackageVersion("2.0.2")),
                    TfF((Net100, EDependencyFlag.Valid))),
                (new Package("NSubstitute", CreatePackageVersion("5.3.0")),
                    TfF((Net100, EDependencyFlag.Valid))),
                (new Package("NSubstitute.Analyzers.CSharp", CreatePackageVersion("1.0.17")),
                    TfF((Net100, EDependencyFlag.Valid))),
                (new Package("System.Text.Json", CreatePackageVersion("10.0.1")),
                    TfF((Net100, EDependencyFlag.Valid))),
                (new Package("System.IO.Pipelines", CreatePackageVersion("9.0.0")),
                    TfF((Net100, EDependencyFlag.Unknown), (Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("9.0.0")),
                    TfF((Net100, EDependencyFlag.Unknown), (Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("System.Buffers", CreatePackageVersion("4.5.1")),
                    TfF((Net100, EDependencyFlag.Unknown), (Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("System.Memory", CreatePackageVersion("4.5.5")),
                    TfF((Net100, EDependencyFlag.Unknown), (Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("System.Runtime.CompilerServices.Unsafe", CreatePackageVersion("6.0.0")),
                    TfF((Net100, EDependencyFlag.Unknown), (Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("System.IO.Pipelines", CreatePackageVersion("9.0.1")),
                    TfF((Net100, EDependencyFlag.Unknown), (Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("9.0.1")),
                    TfF((Net100, EDependencyFlag.Unknown), (Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("System.IO.Pipelines", CreatePackageVersion("9.0.2")),
                    TfF((Net100, EDependencyFlag.Unknown), (Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("9.0.2")),
                    TfF((Net100, EDependencyFlag.Unknown), (Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("System.IO.Pipelines", CreatePackageVersion("9.0.3")),
                    TfF((Net100, EDependencyFlag.Unknown), (Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("9.0.3")),
                    TfF((Net100, EDependencyFlag.Unknown), (Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("System.IO.Pipelines", CreatePackageVersion("9.0.4")),
                    TfF((Net100, EDependencyFlag.Unknown), (Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("9.0.4")),
                    TfF((Net100, EDependencyFlag.Unknown), (Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("System.IO.Pipelines", CreatePackageVersion("9.0.5")),
                    TfF((Net100, EDependencyFlag.Unknown), (Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("9.0.5")),
                    TfF((Net100, EDependencyFlag.Unknown), (Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("System.IO.Pipelines", CreatePackageVersion("9.0.6")),
                    TfF((Net100, EDependencyFlag.Unknown), (Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("9.0.6")),
                    TfF((Net100, EDependencyFlag.Unknown), (Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("System.IO.Pipelines", CreatePackageVersion("9.0.7")),
                    TfF((Net100, EDependencyFlag.Unknown), (Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("9.0.7")),
                    TfF((Net100, EDependencyFlag.Unknown), (Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("System.IO.Pipelines", CreatePackageVersion("9.0.8")),
                    TfF((Net100, EDependencyFlag.Unknown), (Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("9.0.8")),
                    TfF((Net100, EDependencyFlag.Unknown), (Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("System.IO.Pipelines", CreatePackageVersion("9.0.9")),
                    TfF((Net100, EDependencyFlag.Unknown), (Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("9.0.9")),
                    TfF((Net100, EDependencyFlag.Unknown), (Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("System.IO.Pipelines", CreatePackageVersion("9.0.10")),
                    TfF((Net100, EDependencyFlag.Unknown), (Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("9.0.10")),
                    TfF((Net100, EDependencyFlag.Unknown), (Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("System.IO.Pipelines", CreatePackageVersion("9.0.11")),
                    TfF((Net100, EDependencyFlag.Unknown), (Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("9.0.11")),
                    TfF((Net100, EDependencyFlag.Unknown), (Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("System.IO.Pipelines", CreatePackageVersion("10.0.0")),
                    TfF((Net100, EDependencyFlag.Unknown), (Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("10.0.0")),
                    TfF((Net100, EDependencyFlag.Unknown), (Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("System.Buffers", CreatePackageVersion("4.6.1")),
                    TfF((Net100, EDependencyFlag.Unknown), (Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("System.Memory", CreatePackageVersion("4.6.3")),
                    TfF((Net100, EDependencyFlag.Unknown), (Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("System.Runtime.CompilerServices.Unsafe", CreatePackageVersion("6.1.2")),
                    TfF((Net100, EDependencyFlag.Unknown), (Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("System.IO.Pipelines", CreatePackageVersion("10.0.1")),
                    TfF((Net100, EDependencyFlag.Unknown), (Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("10.0.1")),
                    TfF((Net100, EDependencyFlag.Unknown), (Net80, EDependencyFlag.Unknown), (Net90, EDependencyFlag.Unknown))),
                (new Package("System.Text.Json", CreatePackageVersion("8.0.6")),
                    TfF((Net80, EDependencyFlag.Valid))),
                (new Package("System.Text.Json", CreatePackageVersion("9.0.11")),
                    TfF((Net90, EDependencyFlag.Valid))),
                (new Package("ZLinq", CreatePackageVersion("1.5.4")),
                    TfF((Net100, EDependencyFlag.Valid), (Net80, EDependencyFlag.Valid), (Net90, EDependencyFlag.Valid))),
                (new Package("ZeroQL", CreatePackageVersion("8.0.0-preview.7")),
                    TfF((Net80, EDependencyFlag.Valid), (Net90, EDependencyFlag.Valid), (Net100, EDependencyFlag.Valid))),
                (new Package("microsoft.openapi.kiota", CreatePackageVersion("1.29.0")),
                    TfF((Net100, EDependencyFlag.Valid))),
                (new Package("xunit.v3.mtp-v2", CreatePackageVersion("3.2.1")),
                    TfF((Net100, EDependencyFlag.Valid))),
                (new Package("zeroql.cli", CreatePackageVersion("8.0.0-preview.7")),
                    TfF((Net100, EDependencyFlag.Valid)))
            ),
            new Queue<(Package, IReadOnlyCollection<NugetTargetFramework>)>([
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("10.0.1")), [Net80]),
                (new Package("xunit.v3.extensibility.core", CreatePackageVersion("3.2.1")), [Net80]),
                (new Package("Microsoft.Net.Http.Headers", CreatePackageVersion("9.0.0")), [Net90]),
                (new Package("Microsoft.Net.Http.Headers", CreatePackageVersion("9.0.1")), [Net90]),
                (new Package("Microsoft.Net.Http.Headers", CreatePackageVersion("9.0.2")), [Net90]),
                (new Package("Microsoft.Net.Http.Headers", CreatePackageVersion("9.0.3")), [Net90]),
                (new Package("Microsoft.Net.Http.Headers", CreatePackageVersion("9.0.4")), [Net90]),
                (new Package("Microsoft.Net.Http.Headers", CreatePackageVersion("9.0.5")), [Net90]),
                (new Package("Microsoft.Net.Http.Headers", CreatePackageVersion("9.0.6")), [Net90]),
                (new Package("Microsoft.Net.Http.Headers", CreatePackageVersion("9.0.7")), [Net90]),
                (new Package("Microsoft.Net.Http.Headers", CreatePackageVersion("9.0.8")), [Net90]),
                (new Package("Microsoft.Net.Http.Headers", CreatePackageVersion("9.0.9")), [Net90]),
                (new Package("Microsoft.Net.Http.Headers", CreatePackageVersion("9.0.10")), [Net90]),
                (new Package("Microsoft.Net.Http.Headers", CreatePackageVersion("9.0.11")), [Net90]),
                (new Package("Microsoft.Net.Http.Headers", CreatePackageVersion("9.0.0")), [Net100]),
                (new Package("Microsoft.Net.Http.Headers", CreatePackageVersion("9.0.1")), [Net100]),
                (new Package("Microsoft.Net.Http.Headers", CreatePackageVersion("9.0.2")), [Net100]),
                (new Package("Microsoft.Net.Http.Headers", CreatePackageVersion("9.0.3")), [Net100]),
                (new Package("Microsoft.Net.Http.Headers", CreatePackageVersion("9.0.4")), [Net100]),
                (new Package("Microsoft.Net.Http.Headers", CreatePackageVersion("9.0.5")), [Net100]),
                (new Package("Microsoft.Net.Http.Headers", CreatePackageVersion("9.0.6")), [Net100]),
                (new Package("Microsoft.Net.Http.Headers", CreatePackageVersion("9.0.7")), [Net100]),
                (new Package("Microsoft.Net.Http.Headers", CreatePackageVersion("9.0.8")), [Net100]),
                (new Package("Microsoft.Net.Http.Headers", CreatePackageVersion("9.0.9")), [Net100]),
                (new Package("Microsoft.Net.Http.Headers", CreatePackageVersion("9.0.10")), [Net100]),
                (new Package("Microsoft.Net.Http.Headers", CreatePackageVersion("9.0.11")), [Net100]),
                (new Package("Microsoft.Net.Http.Headers", CreatePackageVersion("10.0.0")), [Net100]),
                (new Package("Microsoft.Net.Http.Headers", CreatePackageVersion("10.0.1")), [Net100]),
                (new Package("System.Threading.Tasks.Extensions", CreatePackageVersion("4.6.3")), [Net100]),
                (new Package("System.Threading.Tasks.Extensions", CreatePackageVersion("4.6.3")), [Net90]),
                (new Package("Microsoft.Extensions.Primitives", CreatePackageVersion("9.0.0")), [Net80]),
                (new Package("System.Threading.Tasks.Extensions", CreatePackageVersion("4.5.4")), [Net80]),
                (new Package("Microsoft.Extensions.Primitives", CreatePackageVersion("9.0.1")), [Net80]),
                (new Package("Microsoft.Extensions.Primitives", CreatePackageVersion("9.0.2")), [Net80]),
                (new Package("Microsoft.Extensions.Primitives", CreatePackageVersion("9.0.3")), [Net80]),
                (new Package("Microsoft.Extensions.Primitives", CreatePackageVersion("9.0.4")), [Net80]),
                (new Package("Microsoft.Extensions.Primitives", CreatePackageVersion("9.0.5")), [Net80]),
                (new Package("Microsoft.Extensions.Primitives", CreatePackageVersion("9.0.6")), [Net80]),
                (new Package("Microsoft.Extensions.Primitives", CreatePackageVersion("9.0.7")), [Net80]),
                (new Package("Microsoft.Extensions.Primitives", CreatePackageVersion("9.0.8")), [Net80]),
                (new Package("Microsoft.Extensions.Primitives", CreatePackageVersion("9.0.9")), [Net80]),
                (new Package("Microsoft.Extensions.Primitives", CreatePackageVersion("9.0.10")), [Net80]),
                (new Package("Microsoft.Extensions.Primitives", CreatePackageVersion("9.0.11")), [Net80]),
                (new Package("Microsoft.Extensions.Primitives", CreatePackageVersion("10.0.0")), [Net80]),
                (new Package("System.Threading.Tasks.Extensions", CreatePackageVersion("4.6.3")), [Net80]),
                (new Package("Microsoft.Extensions.Primitives", CreatePackageVersion("10.0.1")), [Net80]),
                (new Package("Microsoft.Extensions.Primitives", CreatePackageVersion("9.0.0")), [Net100]),
                (new Package("System.Threading.Tasks.Extensions", CreatePackageVersion("4.5.4")), [Net100]),
                (new Package("Microsoft.Extensions.Primitives", CreatePackageVersion("9.0.1")), [Net100]),
                (new Package("Microsoft.Extensions.Primitives", CreatePackageVersion("9.0.2")), [Net100]),
                (new Package("Microsoft.Extensions.Primitives", CreatePackageVersion("9.0.3")), [Net100]),
                (new Package("Microsoft.Extensions.Primitives", CreatePackageVersion("9.0.4")), [Net100]),
                (new Package("Microsoft.Extensions.Primitives", CreatePackageVersion("9.0.5")), [Net100]),
                (new Package("Microsoft.Extensions.Primitives", CreatePackageVersion("9.0.6")), [Net100]),
                (new Package("Microsoft.Extensions.Primitives", CreatePackageVersion("9.0.7")), [Net100]),
                (new Package("Microsoft.Extensions.Primitives", CreatePackageVersion("9.0.8")), [Net100]),
                (new Package("Microsoft.Extensions.Primitives", CreatePackageVersion("9.0.9")), [Net100]),
                (new Package("Microsoft.Extensions.Primitives", CreatePackageVersion("9.0.10")), [Net100]),
                (new Package("Microsoft.Extensions.Primitives", CreatePackageVersion("9.0.11")), [Net100]),
                (new Package("Microsoft.Extensions.Primitives", CreatePackageVersion("10.0.0")), [Net100]),
                (new Package("Microsoft.Extensions.Primitives", CreatePackageVersion("10.0.1")), [Net100]),
                (new Package("Microsoft.Extensions.Primitives", CreatePackageVersion("9.0.0")), [Net90]),
                (new Package("System.Threading.Tasks.Extensions", CreatePackageVersion("4.5.4")), [Net90]),
                (new Package("Microsoft.Extensions.Primitives", CreatePackageVersion("9.0.1")), [Net90]),
                (new Package("Microsoft.Extensions.Primitives", CreatePackageVersion("9.0.2")), [Net90]),
                (new Package("Microsoft.Extensions.Primitives", CreatePackageVersion("9.0.3")), [Net90]),
                (new Package("Microsoft.Extensions.Primitives", CreatePackageVersion("9.0.4")), [Net90]),
                (new Package("Microsoft.Extensions.Primitives", CreatePackageVersion("9.0.5")), [Net90]),
                (new Package("Microsoft.Extensions.Primitives", CreatePackageVersion("9.0.6")), [Net90]),
                (new Package("Microsoft.Extensions.Primitives", CreatePackageVersion("9.0.7")), [Net90]),
                (new Package("Microsoft.Extensions.Primitives", CreatePackageVersion("9.0.8")), [Net90]),
                (new Package("Microsoft.Extensions.Primitives", CreatePackageVersion("9.0.9")), [Net90]),
                (new Package("Microsoft.Extensions.Primitives", CreatePackageVersion("9.0.10")), [Net90]),
                (new Package("Microsoft.Extensions.Primitives", CreatePackageVersion("9.0.11")), [Net90]),
                (new Package("Microsoft.Extensions.Primitives", CreatePackageVersion("10.0.0")), [Net90]),
                (new Package("Microsoft.Extensions.Primitives", CreatePackageVersion("10.0.1")), [Net90]),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("9.0.0")), [Net80]),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("9.0.0")), [Net80]),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("9.0.1")), [Net80]),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("9.0.1")), [Net80]),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("9.0.2")), [Net80]),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("9.0.2")), [Net80]),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("9.0.3")), [Net80]),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("9.0.3")), [Net80]),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("9.0.4")), [Net80]),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("9.0.4")), [Net80]),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("9.0.5")), [Net80]),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("9.0.5")), [Net80]),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("9.0.6")), [Net80]),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("9.0.6")), [Net80]),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("9.0.7")), [Net80]),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("9.0.7")), [Net80]),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("9.0.8")), [Net80]),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("9.0.8")), [Net80]),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("9.0.9")), [Net80]),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("9.0.9")), [Net80]),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("9.0.10")), [Net80]),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("9.0.10")), [Net80]),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("9.0.11")), [Net80]),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("9.0.11")), [Net80]),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("10.0.0")), [Net80]),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("10.0.0")), [Net80]),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("10.0.1")), [Net80]),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("10.0.1")), [Net80]),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("9.0.0")), [Net90]),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("9.0.0")), [Net90]),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("9.0.1")), [Net90]),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("9.0.1")), [Net90]),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("9.0.2")), [Net90]),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("9.0.2")), [Net90]),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("9.0.3")), [Net90]),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("9.0.3")), [Net90]),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("9.0.4")), [Net90]),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("9.0.4")), [Net90]),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("9.0.5")), [Net90]),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("9.0.5")), [Net90]),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("9.0.6")), [Net90]),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("9.0.6")), [Net90]),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("9.0.7")), [Net90]),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("9.0.7")), [Net90]),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("9.0.8")), [Net90]),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("9.0.8")), [Net90]),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("9.0.9")), [Net90]),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("9.0.9")), [Net90]),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("9.0.10")), [Net90]),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("9.0.10")), [Net90]),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("10.0.0")), [Net90]),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("10.0.0")), [Net90]),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("10.0.1")), [Net90]),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("10.0.1")), [Net90]),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("9.0.0")), [Net100]),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("9.0.0")), [Net100]),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("9.0.1")), [Net100]),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("9.0.1")), [Net100]),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("9.0.2")), [Net100]),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("9.0.2")), [Net100]),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("9.0.3")), [Net100]),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("9.0.3")), [Net100]),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("9.0.4")), [Net100]),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("9.0.4")), [Net100]),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("9.0.5")), [Net100]),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("9.0.5")), [Net100]),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("9.0.6")), [Net100]),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("9.0.6")), [Net100]),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("9.0.7")), [Net100]),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("9.0.7")), [Net100]),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("9.0.8")), [Net100]),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("9.0.8")), [Net100]),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("9.0.9")), [Net100]),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("9.0.9")), [Net100]),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("9.0.10")), [Net100]),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("9.0.10")), [Net100]),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("9.0.11")), [Net100]),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("9.0.11")), [Net100]),
                (new Package("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("10.0.0")), [Net100]),
                (new Package("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("10.0.0")), [Net100]),
                (new Package("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("9.0.0")), [Net80]),
                (new Package("Microsoft.Extensions.Diagnostics", CreatePackageVersion("9.0.0")), [Net80]),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("9.0.0")), [Net80]),
                (new Package("Microsoft.Extensions.Logging", CreatePackageVersion("9.0.0")), [Net80]),
                (new Package("Microsoft.Extensions.Options", CreatePackageVersion("9.0.0")), [Net80]),
                (new Package("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("9.0.1")), [Net80]),
                (new Package("Microsoft.Extensions.Diagnostics", CreatePackageVersion("9.0.1")), [Net80]),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("9.0.1")), [Net80]),
                (new Package("Microsoft.Extensions.Logging", CreatePackageVersion("9.0.1")), [Net80]),
                (new Package("Microsoft.Extensions.Options", CreatePackageVersion("9.0.1")), [Net80]),
                (new Package("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("9.0.2")), [Net80]),
                (new Package("Microsoft.Extensions.Diagnostics", CreatePackageVersion("9.0.2")), [Net80]),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("9.0.2")), [Net80]),
                (new Package("Microsoft.Extensions.Logging", CreatePackageVersion("9.0.2")), [Net80]),
                (new Package("Microsoft.Extensions.Options", CreatePackageVersion("9.0.2")), [Net80]),
                (new Package("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("9.0.3")), [Net80]),
                (new Package("Microsoft.Extensions.Diagnostics", CreatePackageVersion("9.0.3")), [Net80]),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("9.0.3")), [Net80]),
                (new Package("Microsoft.Extensions.Logging", CreatePackageVersion("9.0.3")), [Net80]),
                (new Package("Microsoft.Extensions.Options", CreatePackageVersion("9.0.3")), [Net80]),
                (new Package("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("9.0.4")), [Net80]),
                (new Package("Microsoft.Extensions.Diagnostics", CreatePackageVersion("9.0.4")), [Net80]),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("9.0.4")), [Net80]),
                (new Package("Microsoft.Extensions.Logging", CreatePackageVersion("9.0.4")), [Net80]),
                (new Package("Microsoft.Extensions.Options", CreatePackageVersion("9.0.4")), [Net80]),
                (new Package("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("9.0.5")), [Net80]),
                (new Package("Microsoft.Extensions.Diagnostics", CreatePackageVersion("9.0.5")), [Net80]),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("9.0.5")), [Net80]),
                (new Package("Microsoft.Extensions.Logging", CreatePackageVersion("9.0.5")), [Net80]),
                (new Package("Microsoft.Extensions.Options", CreatePackageVersion("9.0.5")), [Net80]),
                (new Package("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("9.0.6")), [Net80]),
                (new Package("Microsoft.Extensions.Diagnostics", CreatePackageVersion("9.0.6")), [Net80]),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("9.0.6")), [Net80]),
                (new Package("Microsoft.Extensions.Logging", CreatePackageVersion("9.0.6")), [Net80]),
                (new Package("Microsoft.Extensions.Options", CreatePackageVersion("9.0.6")), [Net80]),
                (new Package("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("9.0.7")), [Net80]),
                (new Package("Microsoft.Extensions.Diagnostics", CreatePackageVersion("9.0.7")), [Net80]),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("9.0.7")), [Net80]),
                (new Package("Microsoft.Extensions.Logging", CreatePackageVersion("9.0.7")), [Net80]),
                (new Package("Microsoft.Extensions.Options", CreatePackageVersion("9.0.7")), [Net80]),
                (new Package("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("9.0.8")), [Net80]),
                (new Package("Microsoft.Extensions.Diagnostics", CreatePackageVersion("9.0.8")), [Net80]),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("9.0.8")), [Net80]),
                (new Package("Microsoft.Extensions.Logging", CreatePackageVersion("9.0.8")), [Net80]),
                (new Package("Microsoft.Extensions.Options", CreatePackageVersion("9.0.8")), [Net80]),
                (new Package("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("9.0.9")), [Net80]),
                (new Package("Microsoft.Extensions.Diagnostics", CreatePackageVersion("9.0.9")), [Net80]),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("9.0.9")), [Net80]),
                (new Package("Microsoft.Extensions.Logging", CreatePackageVersion("9.0.9")), [Net80]),
                (new Package("Microsoft.Extensions.Options", CreatePackageVersion("9.0.9")), [Net80]),
                (new Package("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("9.0.10")), [Net80]),
                (new Package("Microsoft.Extensions.Diagnostics", CreatePackageVersion("9.0.10")), [Net80]),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("9.0.10")), [Net80]),
                (new Package("Microsoft.Extensions.Logging", CreatePackageVersion("9.0.10")), [Net80]),
                (new Package("Microsoft.Extensions.Options", CreatePackageVersion("9.0.10")), [Net80]),
                (new Package("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("9.0.11")), [Net80]),
                (new Package("Microsoft.Extensions.Diagnostics", CreatePackageVersion("9.0.11")), [Net80]),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("9.0.11")), [Net80]),
                (new Package("Microsoft.Extensions.Logging", CreatePackageVersion("9.0.11")), [Net80]),
                (new Package("Microsoft.Extensions.Options", CreatePackageVersion("9.0.11")), [Net80]),
                (new Package("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("10.0.0")), [Net80]),
                (new Package("Microsoft.Extensions.Diagnostics", CreatePackageVersion("10.0.0")), [Net80]),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("10.0.0")), [Net80]),
                (new Package("Microsoft.Extensions.Logging", CreatePackageVersion("10.0.0")), [Net80]),
                (new Package("Microsoft.Extensions.Options", CreatePackageVersion("10.0.0")), [Net80]),
                (new Package("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("10.0.1")), [Net80]),
                (new Package("Microsoft.Extensions.Diagnostics", CreatePackageVersion("10.0.1")), [Net80]),
                (new Package("Microsoft.Extensions.Logging", CreatePackageVersion("10.0.1")), [Net80]),
                (new Package("Microsoft.Extensions.Options", CreatePackageVersion("10.0.1")), [Net80]),
                (new Package("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("9.0.0")), [Net100]),
                (new Package("Microsoft.Extensions.Diagnostics", CreatePackageVersion("9.0.0")), [Net100]),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("9.0.0")), [Net100]),
                (new Package("Microsoft.Extensions.Logging", CreatePackageVersion("9.0.0")), [Net100]),
                (new Package("Microsoft.Extensions.Options", CreatePackageVersion("9.0.0")), [Net100]),
                (new Package("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("9.0.1")), [Net100]),
                (new Package("Microsoft.Extensions.Diagnostics", CreatePackageVersion("9.0.1")), [Net100]),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("9.0.1")), [Net100]),
                (new Package("Microsoft.Extensions.Logging", CreatePackageVersion("9.0.1")), [Net100]),
                (new Package("Microsoft.Extensions.Options", CreatePackageVersion("9.0.1")), [Net100]),
                (new Package("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("9.0.2")), [Net100]),
                (new Package("Microsoft.Extensions.Diagnostics", CreatePackageVersion("9.0.2")), [Net100]),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("9.0.2")), [Net100]),
                (new Package("Microsoft.Extensions.Logging", CreatePackageVersion("9.0.2")), [Net100]),
                (new Package("Microsoft.Extensions.Options", CreatePackageVersion("9.0.2")), [Net100]),
                (new Package("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("9.0.3")), [Net100]),
                (new Package("Microsoft.Extensions.Diagnostics", CreatePackageVersion("9.0.3")), [Net100]),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("9.0.3")), [Net100]),
                (new Package("Microsoft.Extensions.Logging", CreatePackageVersion("9.0.3")), [Net100]),
                (new Package("Microsoft.Extensions.Options", CreatePackageVersion("9.0.3")), [Net100]),
                (new Package("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("9.0.4")), [Net100]),
                (new Package("Microsoft.Extensions.Diagnostics", CreatePackageVersion("9.0.4")), [Net100]),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("9.0.4")), [Net100]),
                (new Package("Microsoft.Extensions.Logging", CreatePackageVersion("9.0.4")), [Net100]),
                (new Package("Microsoft.Extensions.Options", CreatePackageVersion("9.0.4")), [Net100]),
                (new Package("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("9.0.5")), [Net100]),
                (new Package("Microsoft.Extensions.Diagnostics", CreatePackageVersion("9.0.5")), [Net100]),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("9.0.5")), [Net100]),
                (new Package("Microsoft.Extensions.Logging", CreatePackageVersion("9.0.5")), [Net100]),
                (new Package("Microsoft.Extensions.Options", CreatePackageVersion("9.0.5")), [Net100]),
                (new Package("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("9.0.6")), [Net100]),
                (new Package("Microsoft.Extensions.Diagnostics", CreatePackageVersion("9.0.6")), [Net100]),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("9.0.6")), [Net100]),
                (new Package("Microsoft.Extensions.Logging", CreatePackageVersion("9.0.6")), [Net100]),
                (new Package("Microsoft.Extensions.Options", CreatePackageVersion("9.0.6")), [Net100]),
                (new Package("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("9.0.7")), [Net100]),
                (new Package("Microsoft.Extensions.Diagnostics", CreatePackageVersion("9.0.7")), [Net100]),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("9.0.7")), [Net100]),
                (new Package("Microsoft.Extensions.Logging", CreatePackageVersion("9.0.7")), [Net100]),
                (new Package("Microsoft.Extensions.Options", CreatePackageVersion("9.0.7")), [Net100]),
                (new Package("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("9.0.8")), [Net100]),
                (new Package("Microsoft.Extensions.Diagnostics", CreatePackageVersion("9.0.8")), [Net100]),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("9.0.8")), [Net100]),
                (new Package("Microsoft.Extensions.Logging", CreatePackageVersion("9.0.8")), [Net100]),
                (new Package("Microsoft.Extensions.Options", CreatePackageVersion("9.0.8")), [Net100]),
                (new Package("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("9.0.9")), [Net100]),
                (new Package("Microsoft.Extensions.Diagnostics", CreatePackageVersion("9.0.9")), [Net100]),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("9.0.9")), [Net100]),
                (new Package("Microsoft.Extensions.Logging", CreatePackageVersion("9.0.9")), [Net100]),
                (new Package("Microsoft.Extensions.Options", CreatePackageVersion("9.0.9")), [Net100]),
                (new Package("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("9.0.10")), [Net100]),
                (new Package("Microsoft.Extensions.Diagnostics", CreatePackageVersion("9.0.10")), [Net100]),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("9.0.10")), [Net100]),
                (new Package("Microsoft.Extensions.Logging", CreatePackageVersion("9.0.10")), [Net100]),
                (new Package("Microsoft.Extensions.Options", CreatePackageVersion("9.0.10")), [Net100]),
                (new Package("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("9.0.11")), [Net100]),
                (new Package("Microsoft.Extensions.Diagnostics", CreatePackageVersion("9.0.11")), [Net100]),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("9.0.11")), [Net100]),
                (new Package("Microsoft.Extensions.Logging", CreatePackageVersion("9.0.11")), [Net100]),
                (new Package("Microsoft.Extensions.Options", CreatePackageVersion("9.0.11")), [Net100]),
                (new Package("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("10.0.0")), [Net100]),
                (new Package("Microsoft.Extensions.Diagnostics", CreatePackageVersion("10.0.0")), [Net100]),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("10.0.0")), [Net100]),
                (new Package("Microsoft.Extensions.Logging", CreatePackageVersion("10.0.0")), [Net100]),
                (new Package("Microsoft.Extensions.Options", CreatePackageVersion("10.0.0")), [Net100]),
                (new Package("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("10.0.1")), [Net100]),
                (new Package("Microsoft.Extensions.Diagnostics", CreatePackageVersion("10.0.1")), [Net100]),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("10.0.1")), [Net100]),
                (new Package("Microsoft.Extensions.Logging", CreatePackageVersion("10.0.1")), [Net100]),
                (new Package("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("9.0.0")), [Net90]),
                (new Package("Microsoft.Extensions.Diagnostics", CreatePackageVersion("9.0.0")), [Net90]),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("9.0.0")), [Net90]),
                (new Package("Microsoft.Extensions.Logging", CreatePackageVersion("9.0.0")), [Net90]),
                (new Package("Microsoft.Extensions.Options", CreatePackageVersion("9.0.0")), [Net90]),
                (new Package("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("9.0.1")), [Net90]),
                (new Package("Microsoft.Extensions.Diagnostics", CreatePackageVersion("9.0.1")), [Net90]),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("9.0.1")), [Net90]),
                (new Package("Microsoft.Extensions.Logging", CreatePackageVersion("9.0.1")), [Net90]),
                (new Package("Microsoft.Extensions.Options", CreatePackageVersion("9.0.1")), [Net90]),
                (new Package("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("9.0.2")), [Net90]),
                (new Package("Microsoft.Extensions.Diagnostics", CreatePackageVersion("9.0.2")), [Net90]),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("9.0.2")), [Net90]),
                (new Package("Microsoft.Extensions.Logging", CreatePackageVersion("9.0.2")), [Net90]),
                (new Package("Microsoft.Extensions.Options", CreatePackageVersion("9.0.2")), [Net90]),
                (new Package("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("9.0.3")), [Net90]),
                (new Package("Microsoft.Extensions.Diagnostics", CreatePackageVersion("9.0.3")), [Net90]),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("9.0.3")), [Net90]),
                (new Package("Microsoft.Extensions.Logging", CreatePackageVersion("9.0.3")), [Net90]),
                (new Package("Microsoft.Extensions.Options", CreatePackageVersion("9.0.3")), [Net90]),
                (new Package("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("9.0.4")), [Net90]),
                (new Package("Microsoft.Extensions.Diagnostics", CreatePackageVersion("9.0.4")), [Net90]),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("9.0.4")), [Net90]),
                (new Package("Microsoft.Extensions.Logging", CreatePackageVersion("9.0.4")), [Net90]),
                (new Package("Microsoft.Extensions.Options", CreatePackageVersion("9.0.4")), [Net90]),
                (new Package("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("9.0.5")), [Net90]),
                (new Package("Microsoft.Extensions.Diagnostics", CreatePackageVersion("9.0.5")), [Net90]),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("9.0.5")), [Net90]),
                (new Package("Microsoft.Extensions.Logging", CreatePackageVersion("9.0.5")), [Net90]),
                (new Package("Microsoft.Extensions.Options", CreatePackageVersion("9.0.5")), [Net90]),
                (new Package("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("9.0.6")), [Net90]),
                (new Package("Microsoft.Extensions.Diagnostics", CreatePackageVersion("9.0.6")), [Net90]),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("9.0.6")), [Net90]),
                (new Package("Microsoft.Extensions.Logging", CreatePackageVersion("9.0.6")), [Net90]),
                (new Package("Microsoft.Extensions.Options", CreatePackageVersion("9.0.6")), [Net90]),
                (new Package("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("9.0.7")), [Net90]),
                (new Package("Microsoft.Extensions.Diagnostics", CreatePackageVersion("9.0.7")), [Net90]),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("9.0.7")), [Net90]),
                (new Package("Microsoft.Extensions.Logging", CreatePackageVersion("9.0.7")), [Net90]),
                (new Package("Microsoft.Extensions.Options", CreatePackageVersion("9.0.7")), [Net90]),
                (new Package("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("9.0.8")), [Net90]),
                (new Package("Microsoft.Extensions.Diagnostics", CreatePackageVersion("9.0.8")), [Net90]),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("9.0.8")), [Net90]),
                (new Package("Microsoft.Extensions.Logging", CreatePackageVersion("9.0.8")), [Net90]),
                (new Package("Microsoft.Extensions.Options", CreatePackageVersion("9.0.8")), [Net90]),
                (new Package("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("9.0.9")), [Net90]),
                (new Package("Microsoft.Extensions.Diagnostics", CreatePackageVersion("9.0.9")), [Net90]),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("9.0.9")), [Net90]),
                (new Package("Microsoft.Extensions.Logging", CreatePackageVersion("9.0.9")), [Net90]),
                (new Package("Microsoft.Extensions.Options", CreatePackageVersion("9.0.9")), [Net90]),
                (new Package("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("9.0.10")), [Net90]),
                (new Package("Microsoft.Extensions.Diagnostics", CreatePackageVersion("9.0.10")), [Net90]),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("9.0.10")), [Net90]),
                (new Package("Microsoft.Extensions.Logging", CreatePackageVersion("9.0.10")), [Net90]),
                (new Package("Microsoft.Extensions.Options", CreatePackageVersion("9.0.10")), [Net90]),
                (new Package("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("9.0.11")), [Net90]),
                (new Package("Microsoft.Extensions.Diagnostics", CreatePackageVersion("9.0.11")), [Net90]),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("9.0.11")), [Net90]),
                (new Package("Microsoft.Extensions.Logging", CreatePackageVersion("9.0.11")), [Net90]),
                (new Package("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("10.0.0")), [Net90]),
                (new Package("Microsoft.Extensions.Diagnostics", CreatePackageVersion("10.0.0")), [Net90]),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("10.0.0")), [Net90]),
                (new Package("Microsoft.Extensions.Logging", CreatePackageVersion("10.0.0")), [Net90]),
                (new Package("Microsoft.Extensions.Options", CreatePackageVersion("10.0.0")), [Net90]),
                (new Package("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("10.0.1")), [Net90]),
                (new Package("Microsoft.Extensions.Diagnostics", CreatePackageVersion("10.0.1")), [Net90]),
                (new Package("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("10.0.1")), [Net90]),
                (new Package("Microsoft.Extensions.Logging", CreatePackageVersion("10.0.1")), [Net90]),
                (new Package("Microsoft.Extensions.Options", CreatePackageVersion("10.0.1")), [Net90]),
                (new Package("System.ComponentModel.Annotations", CreatePackageVersion("5.0.0")), [Net80]),
                (new Package("System.ComponentModel.Annotations", CreatePackageVersion("5.0.0")), [Net90]),
                (new Package("System.ComponentModel.Annotations", CreatePackageVersion("5.0.0")), [Net100]),
                (new Package("Std.UriTemplate", CreatePackageVersion("2.0.8")), [Net80, Net90, Net100]),
                (new Package("System.Diagnostics.DiagnosticSource", CreatePackageVersion("6.0.0")), [Net80, Net90, Net100]),
                (new Package("Microsoft.Kiota.Abstractions", CreatePackageVersion("1.21.1")), [Net80, Net90, Net100]),
                (new Package("System.Text.Json", CreatePackageVersion("6.0.10")), [Net80, Net90, Net100]),
                (new Package("System.IO.Pipelines", CreatePackageVersion("9.0.0")), [Net100]),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("9.0.0")), [Net100]),
                (new Package("System.Buffers", CreatePackageVersion("4.5.1")), [Net100]),
                (new Package("System.Memory", CreatePackageVersion("4.5.5")), [Net100]),
                (new Package("System.Runtime.CompilerServices.Unsafe", CreatePackageVersion("6.0.0")), [Net100]),
                (new Package("System.IO.Pipelines", CreatePackageVersion("9.0.1")), [Net100]),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("9.0.1")), [Net100]),
                (new Package("System.IO.Pipelines", CreatePackageVersion("9.0.2")), [Net100]),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("9.0.2")), [Net100]),
                (new Package("System.IO.Pipelines", CreatePackageVersion("9.0.3")), [Net100]),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("9.0.3")), [Net100]),
                (new Package("System.IO.Pipelines", CreatePackageVersion("9.0.4")), [Net100]),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("9.0.4")), [Net100]),
                (new Package("System.IO.Pipelines", CreatePackageVersion("9.0.5")), [Net100]),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("9.0.5")), [Net100]),
                (new Package("System.IO.Pipelines", CreatePackageVersion("9.0.6")), [Net100]),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("9.0.6")), [Net100]),
                (new Package("System.IO.Pipelines", CreatePackageVersion("9.0.7")), [Net100]),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("9.0.7")), [Net100]),
                (new Package("System.IO.Pipelines", CreatePackageVersion("9.0.8")), [Net100]),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("9.0.8")), [Net100]),
                (new Package("System.IO.Pipelines", CreatePackageVersion("9.0.9")), [Net100]),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("9.0.9")), [Net100]),
                (new Package("System.IO.Pipelines", CreatePackageVersion("9.0.10")), [Net100]),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("9.0.10")), [Net100]),
                (new Package("System.IO.Pipelines", CreatePackageVersion("9.0.11")), [Net100]),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("9.0.11")), [Net100]),
                (new Package("System.IO.Pipelines", CreatePackageVersion("10.0.0")), [Net100]),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("10.0.0")), [Net100]),
                (new Package("System.Buffers", CreatePackageVersion("4.6.1")), [Net100]),
                (new Package("System.Memory", CreatePackageVersion("4.6.3")), [Net100]),
                (new Package("System.Runtime.CompilerServices.Unsafe", CreatePackageVersion("6.1.2")), [Net100]),
                (new Package("System.IO.Pipelines", CreatePackageVersion("10.0.1")), [Net100]),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("10.0.1")), [Net100]),
                (new Package("System.IO.Pipelines", CreatePackageVersion("9.0.0")), [Net80]),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("9.0.0")), [Net80]),
                (new Package("System.Buffers", CreatePackageVersion("4.5.1")), [Net80]),
                (new Package("System.Memory", CreatePackageVersion("4.5.5")), [Net80]),
                (new Package("System.Runtime.CompilerServices.Unsafe", CreatePackageVersion("6.0.0")), [Net80]),
                (new Package("System.IO.Pipelines", CreatePackageVersion("9.0.1")), [Net80]),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("9.0.1")), [Net80]),
                (new Package("System.IO.Pipelines", CreatePackageVersion("9.0.2")), [Net80]),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("9.0.2")), [Net80]),
                (new Package("System.IO.Pipelines", CreatePackageVersion("9.0.3")), [Net80]),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("9.0.3")), [Net80]),
                (new Package("System.IO.Pipelines", CreatePackageVersion("9.0.4")), [Net80]),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("9.0.4")), [Net80]),
                (new Package("System.IO.Pipelines", CreatePackageVersion("9.0.5")), [Net80]),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("9.0.5")), [Net80]),
                (new Package("System.IO.Pipelines", CreatePackageVersion("9.0.6")), [Net80]),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("9.0.6")), [Net80]),
                (new Package("System.IO.Pipelines", CreatePackageVersion("9.0.7")), [Net80]),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("9.0.7")), [Net80]),
                (new Package("System.IO.Pipelines", CreatePackageVersion("9.0.8")), [Net80]),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("9.0.8")), [Net80]),
                (new Package("System.IO.Pipelines", CreatePackageVersion("9.0.9")), [Net80]),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("9.0.9")), [Net80]),
                (new Package("System.IO.Pipelines", CreatePackageVersion("9.0.10")), [Net80]),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("9.0.10")), [Net80]),
                (new Package("System.IO.Pipelines", CreatePackageVersion("9.0.11")), [Net80]),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("9.0.11")), [Net80]),
                (new Package("System.IO.Pipelines", CreatePackageVersion("10.0.0")), [Net80]),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("10.0.0")), [Net80]),
                (new Package("System.Buffers", CreatePackageVersion("4.6.1")), [Net80]),
                (new Package("System.Memory", CreatePackageVersion("4.6.3")), [Net80]),
                (new Package("System.Runtime.CompilerServices.Unsafe", CreatePackageVersion("6.1.2")), [Net80]),
                (new Package("System.IO.Pipelines", CreatePackageVersion("10.0.1")), [Net80]),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("10.0.1")), [Net80]),
                (new Package("System.IO.Pipelines", CreatePackageVersion("9.0.0")), [Net90]),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("9.0.0")), [Net90]),
                (new Package("System.Buffers", CreatePackageVersion("4.5.1")), [Net90]),
                (new Package("System.Memory", CreatePackageVersion("4.5.5")), [Net90]),
                (new Package("System.Runtime.CompilerServices.Unsafe", CreatePackageVersion("6.0.0")), [Net90]),
                (new Package("System.IO.Pipelines", CreatePackageVersion("9.0.1")), [Net90]),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("9.0.1")), [Net90]),
                (new Package("System.IO.Pipelines", CreatePackageVersion("9.0.2")), [Net90]),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("9.0.2")), [Net90]),
                (new Package("System.IO.Pipelines", CreatePackageVersion("9.0.3")), [Net90]),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("9.0.3")), [Net90]),
                (new Package("System.IO.Pipelines", CreatePackageVersion("9.0.4")), [Net90]),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("9.0.4")), [Net90]),
                (new Package("System.IO.Pipelines", CreatePackageVersion("9.0.5")), [Net90]),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("9.0.5")), [Net90]),
                (new Package("System.IO.Pipelines", CreatePackageVersion("9.0.6")), [Net90]),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("9.0.6")), [Net90]),
                (new Package("System.IO.Pipelines", CreatePackageVersion("9.0.7")), [Net90]),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("9.0.7")), [Net90]),
                (new Package("System.IO.Pipelines", CreatePackageVersion("9.0.8")), [Net90]),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("9.0.8")), [Net90]),
                (new Package("System.IO.Pipelines", CreatePackageVersion("9.0.9")), [Net90]),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("9.0.9")), [Net90]),
                (new Package("System.IO.Pipelines", CreatePackageVersion("9.0.10")), [Net90]),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("9.0.10")), [Net90]),
                (new Package("System.IO.Pipelines", CreatePackageVersion("9.0.11")), [Net90]),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("9.0.11")), [Net90]),
                (new Package("System.IO.Pipelines", CreatePackageVersion("10.0.0")), [Net90]),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("10.0.0")), [Net90]),
                (new Package("System.Buffers", CreatePackageVersion("4.6.1")), [Net90]),
                (new Package("System.Memory", CreatePackageVersion("4.6.3")), [Net90]),
                (new Package("System.Runtime.CompilerServices.Unsafe", CreatePackageVersion("6.1.2")), [Net90]),
                (new Package("System.IO.Pipelines", CreatePackageVersion("10.0.1")), [Net90]),
                (new Package("System.Text.Encodings.Web", CreatePackageVersion("10.0.1")), [Net90])
            ])
        ),
        new Dictionary<string, PackageVersionWithDependencySets?>
        {
            ["Microsoft.Net.Http.Headers=9.0.10.0"] = null,
            ["Microsoft.Net.Http.Headers=9.0.11.0"] = null,
            ["Microsoft.Net.Http.Headers=10.0.1.0"] = null,
            ["Std.UriTemplate=2.0.8.0"] = null,
            ["System.Diagnostics.DiagnosticSource=6.0.0.0"] = null,
        }
    )
)
{
    private static readonly NugetTargetFramework Net48 = new("net48");
    private static readonly NugetTargetFramework Net80 = new("net8.0");
    private static readonly NugetTargetFramework Net90 = new("net9.0");
    private static readonly NugetTargetFramework Net100 = new("net10.0");

    private static PackageVersion CreatePackageVersion(
        string version
    ) => VersionRange.Parse(version).MinVersion!.MapToPackageVersion();

    private static IReadOnlyDictionary<EPackageSource, IReadOnlyCollection<DependencySet>> CreateDependencySets(
        params IReadOnlyCollection<DependencySet> dependencySets
    ) => new Dictionary<EPackageSource, IReadOnlyCollection<DependencySet>>
    {
        [EPackageSource.Default] = dependencySets,
    };

    private static IDictionary<Package, IDictionary<NugetTargetFramework, EDependencyFlag>> CreatePackageFlags(
        params IReadOnlyCollection<(
            Package, IDictionary<NugetTargetFramework, EDependencyFlag>
            )> flags
    )
    {
        var packageFlags = new Dictionary<Package, IDictionary<NugetTargetFramework, EDependencyFlag>>();

        foreach (var (package, targetFrameworkFlags) in flags)
        {
            packageFlags.Add(package, targetFrameworkFlags);
        }

        return packageFlags;
    }

    private static IDictionary<NugetTargetFramework, EDependencyFlag> TfF(
        params IReadOnlyCollection<ValueTuple<NugetTargetFramework, EDependencyFlag>> targetFrameworkFlags
    ) => targetFrameworkFlags.AsValueEnumerable().ToDictionary(
        x => x.Item1,
        x => x.Item2
    );
}
