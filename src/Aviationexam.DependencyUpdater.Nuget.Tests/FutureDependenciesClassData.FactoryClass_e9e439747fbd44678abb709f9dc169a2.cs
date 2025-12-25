using Aviationexam.DependencyUpdater.Common;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Aviationexam.DependencyUpdater.Nuget.Tests;

public sealed partial class FutureDependenciesClassData
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private static class FactoryClass_e9e439747fbd44678abb709f9dc169a2
    {
        public static IReadOnlyCollection<PackageVersionWithDependencySets> CreateDependencySetsMicrosoft_AspNetCore_WebUtilities() =>
        [
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.0"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Net.Http.Headers", CreatePackageVersion("[9.0.0, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.1"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Net.Http.Headers", CreatePackageVersion("[9.0.1, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.2"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Net.Http.Headers", CreatePackageVersion("[9.0.2, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.3"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Net.Http.Headers", CreatePackageVersion("[9.0.3, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.4"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Net.Http.Headers", CreatePackageVersion("[9.0.4, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.5"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Net.Http.Headers", CreatePackageVersion("[9.0.5, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.6"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Net.Http.Headers", CreatePackageVersion("[9.0.6, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.7"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Net.Http.Headers", CreatePackageVersion("[9.0.7, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.8"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Net.Http.Headers", CreatePackageVersion("[9.0.8, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.9"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Net.Http.Headers", CreatePackageVersion("[9.0.9, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.10"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Net.Http.Headers", CreatePackageVersion("[9.0.10, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.11"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Net.Http.Headers", CreatePackageVersion("[9.0.11, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("10.0.0"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net10.0", [
                        new PackageDependencyInfo("Microsoft.Net.Http.Headers", CreatePackageVersion("[10.0.0, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("10.0.1"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net10.0", [
                        new PackageDependencyInfo("Microsoft.Net.Http.Headers", CreatePackageVersion("[10.0.1, )"))
                    ])
                ),
            }
        ];

        public static IReadOnlyCollection<PackageVersionWithDependencySets> CreateDependencySetsMicrosoft_Testing_Extensions_TrxReport() =>
        [
        ];

        public static IReadOnlyCollection<PackageVersionWithDependencySets> CreateDependencySetsMicrosoft_Extensions_DependencyInjection() =>
        [
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.0"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.0, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.0, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.0, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.0, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.0, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.0, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("netstandard2.1", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.0, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.1"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.1, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.1, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.1, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.1, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.1, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.1, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("netstandard2.1", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.1, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.2"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.2, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.2, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.2, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.2, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.2, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.2, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("netstandard2.1", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.2, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.3"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.3, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.3, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.3, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.3, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.3, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.3, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("netstandard2.1", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.3, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.4"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.4, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.4, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.4, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.4, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.4, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.4, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("netstandard2.1", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.4, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.5"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.5, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.5, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.5, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.5, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.5, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.5, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("netstandard2.1", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.5, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.6"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.6, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.6, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.6, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.6, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.6, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.6, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("netstandard2.1", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.6, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.7"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.7, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.7, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.7, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.7, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.7, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.7, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("netstandard2.1", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.7, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.8"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.8, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.8, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.8, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.8, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.8, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.8, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("netstandard2.1", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.8, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.9"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.9, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.9, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.9, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.9, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.9, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.9, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("netstandard2.1", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.9, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.10"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.10, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.10, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.10, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.10, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.10, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.10, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("netstandard2.1", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.10, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.11"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.11, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.11, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.11, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.11, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.11, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.11, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("netstandard2.1", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.11, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("10.0.0"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[10.0.0, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[10.0.0, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.6.3, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[10.0.0, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[10.0.0, )"))
                    ]),
                    new DependencySet("net10.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[10.0.0, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[10.0.0, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[10.0.0, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.6.3, )"))
                    ]),
                    new DependencySet("netstandard2.1", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[10.0.0, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("10.0.1"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[10.0.1, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[10.0.1, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.6.3, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[10.0.1, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[10.0.1, )"))
                    ]),
                    new DependencySet("net10.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[10.0.1, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[10.0.1, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[10.0.1, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.6.3, )"))
                    ]),
                    new DependencySet("netstandard2.1", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[10.0.1, )"))
                    ])
                ),
            }
        ];

        public static IReadOnlyCollection<PackageVersionWithDependencySets> CreateDependencySetsNSubstitute() =>
        [
        ];

        public static IReadOnlyCollection<PackageVersionWithDependencySets> CreateDependencySetsMicrosoft_Extensions_Http() =>
        [
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.0"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[9.0.0, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.0, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[9.0.0, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[9.0.0, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[9.0.0, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[9.0.0, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.0, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Diagnostics", CreatePackageVersion("[9.0.0, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[9.0.0, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[9.0.0, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[9.0.0, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[9.0.0, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.0, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Diagnostics", CreatePackageVersion("[9.0.0, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[9.0.0, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[9.0.0, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[9.0.0, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[9.0.0, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.0, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[9.0.0, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[9.0.0, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[9.0.0, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.1"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[9.0.1, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.1, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[9.0.1, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[9.0.1, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[9.0.1, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[9.0.1, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.1, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Diagnostics", CreatePackageVersion("[9.0.1, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[9.0.1, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[9.0.1, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[9.0.1, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[9.0.1, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.1, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Diagnostics", CreatePackageVersion("[9.0.1, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[9.0.1, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[9.0.1, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[9.0.1, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[9.0.1, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.1, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[9.0.1, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[9.0.1, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[9.0.1, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.2"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[9.0.2, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.2, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[9.0.2, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[9.0.2, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[9.0.2, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[9.0.2, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.2, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Diagnostics", CreatePackageVersion("[9.0.2, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[9.0.2, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[9.0.2, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[9.0.2, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[9.0.2, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.2, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Diagnostics", CreatePackageVersion("[9.0.2, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[9.0.2, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[9.0.2, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[9.0.2, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[9.0.2, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.2, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[9.0.2, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[9.0.2, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[9.0.2, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.3"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[9.0.3, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.3, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[9.0.3, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[9.0.3, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[9.0.3, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[9.0.3, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.3, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Diagnostics", CreatePackageVersion("[9.0.3, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[9.0.3, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[9.0.3, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[9.0.3, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[9.0.3, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.3, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Diagnostics", CreatePackageVersion("[9.0.3, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[9.0.3, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[9.0.3, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[9.0.3, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[9.0.3, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.3, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[9.0.3, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[9.0.3, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[9.0.3, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.4"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[9.0.4, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.4, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[9.0.4, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[9.0.4, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[9.0.4, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[9.0.4, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.4, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Diagnostics", CreatePackageVersion("[9.0.4, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[9.0.4, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[9.0.4, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[9.0.4, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[9.0.4, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.4, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Diagnostics", CreatePackageVersion("[9.0.4, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[9.0.4, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[9.0.4, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[9.0.4, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[9.0.4, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.4, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[9.0.4, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[9.0.4, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[9.0.4, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.5"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[9.0.5, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.5, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[9.0.5, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[9.0.5, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[9.0.5, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[9.0.5, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.5, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Diagnostics", CreatePackageVersion("[9.0.5, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[9.0.5, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[9.0.5, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[9.0.5, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[9.0.5, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.5, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Diagnostics", CreatePackageVersion("[9.0.5, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[9.0.5, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[9.0.5, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[9.0.5, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[9.0.5, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.5, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[9.0.5, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[9.0.5, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[9.0.5, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.6"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[9.0.6, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.6, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[9.0.6, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[9.0.6, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[9.0.6, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[9.0.6, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.6, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Diagnostics", CreatePackageVersion("[9.0.6, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[9.0.6, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[9.0.6, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[9.0.6, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[9.0.6, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.6, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Diagnostics", CreatePackageVersion("[9.0.6, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[9.0.6, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[9.0.6, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[9.0.6, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[9.0.6, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.6, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[9.0.6, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[9.0.6, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[9.0.6, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.7"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[9.0.7, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.7, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[9.0.7, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[9.0.7, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[9.0.7, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[9.0.7, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.7, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Diagnostics", CreatePackageVersion("[9.0.7, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[9.0.7, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[9.0.7, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[9.0.7, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[9.0.7, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.7, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Diagnostics", CreatePackageVersion("[9.0.7, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[9.0.7, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[9.0.7, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[9.0.7, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[9.0.7, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.7, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[9.0.7, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[9.0.7, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[9.0.7, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.8"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[9.0.8, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.8, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[9.0.8, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[9.0.8, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[9.0.8, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[9.0.8, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.8, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Diagnostics", CreatePackageVersion("[9.0.8, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[9.0.8, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[9.0.8, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[9.0.8, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[9.0.8, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.8, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Diagnostics", CreatePackageVersion("[9.0.8, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[9.0.8, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[9.0.8, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[9.0.8, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[9.0.8, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.8, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[9.0.8, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[9.0.8, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[9.0.8, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.9"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[9.0.9, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.9, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[9.0.9, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[9.0.9, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[9.0.9, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[9.0.9, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.9, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Diagnostics", CreatePackageVersion("[9.0.9, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[9.0.9, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[9.0.9, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[9.0.9, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[9.0.9, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.9, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Diagnostics", CreatePackageVersion("[9.0.9, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[9.0.9, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[9.0.9, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[9.0.9, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[9.0.9, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.9, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[9.0.9, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[9.0.9, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[9.0.9, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.10"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[9.0.10, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.10, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[9.0.10, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[9.0.10, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[9.0.10, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[9.0.10, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.10, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Diagnostics", CreatePackageVersion("[9.0.10, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[9.0.10, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[9.0.10, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[9.0.10, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[9.0.10, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.10, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Diagnostics", CreatePackageVersion("[9.0.10, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[9.0.10, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[9.0.10, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[9.0.10, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[9.0.10, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.10, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[9.0.10, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[9.0.10, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[9.0.10, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.11"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[9.0.11, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.11, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[9.0.11, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[9.0.11, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[9.0.11, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[9.0.11, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.11, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Diagnostics", CreatePackageVersion("[9.0.11, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[9.0.11, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[9.0.11, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[9.0.11, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[9.0.11, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.11, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Diagnostics", CreatePackageVersion("[9.0.11, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[9.0.11, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[9.0.11, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[9.0.11, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[9.0.11, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.11, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[9.0.11, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[9.0.11, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[9.0.11, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("10.0.0"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[10.0.0, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[10.0.0, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[10.0.0, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[10.0.0, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[10.0.0, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[10.0.0, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[10.0.0, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Diagnostics", CreatePackageVersion("[10.0.0, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[10.0.0, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[10.0.0, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[10.0.0, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[10.0.0, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[10.0.0, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Diagnostics", CreatePackageVersion("[10.0.0, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[10.0.0, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[10.0.0, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[10.0.0, )"))
                    ]),
                    new DependencySet("net10.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[10.0.0, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[10.0.0, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Diagnostics", CreatePackageVersion("[10.0.0, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[10.0.0, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[10.0.0, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[10.0.0, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[10.0.0, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[10.0.0, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[10.0.0, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[10.0.0, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[10.0.0, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("10.0.1"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[10.0.1, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[10.0.1, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[10.0.1, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[10.0.1, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[10.0.1, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[10.0.1, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[10.0.1, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Diagnostics", CreatePackageVersion("[10.0.1, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[10.0.1, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[10.0.1, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[10.0.1, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[10.0.1, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[10.0.1, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Diagnostics", CreatePackageVersion("[10.0.1, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[10.0.1, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[10.0.1, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[10.0.1, )"))
                    ]),
                    new DependencySet("net10.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[10.0.1, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[10.0.1, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Diagnostics", CreatePackageVersion("[10.0.1, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[10.0.1, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[10.0.1, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[10.0.1, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Configuration.Abstractions", CreatePackageVersion("[10.0.1, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[10.0.1, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[10.0.1, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Logging", CreatePackageVersion("[10.0.1, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Options", CreatePackageVersion("[10.0.1, )"))
                    ])
                ),
            }
        ];

        public static IReadOnlyCollection<PackageVersionWithDependencySets> CreateDependencySetsMicrosoft_Extensions_Caching_Abstractions() =>
        [
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.0"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.0, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.0, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.0, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.0, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.1"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.1, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.1, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.1, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.1, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.2"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.2, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.2, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.2, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.2, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.3"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.3, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.3, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.3, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.3, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.4"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.4, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.4, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.4, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.4, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.5"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.5, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.5, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.5, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.5, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.6"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.6, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.6, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.6, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.6, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.7"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.7, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.7, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.7, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.7, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.8"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.8, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.8, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.8, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.8, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.9"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.9, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.9, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.9, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.9, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.10"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.10, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.10, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.10, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.10, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.11"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.11, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.11, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.11, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.11, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("10.0.0"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[10.0.0, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.6.3, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[10.0.0, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[10.0.0, )"))
                    ]),
                    new DependencySet("net10.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[10.0.0, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[10.0.0, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.6.3, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("10.0.1"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[10.0.1, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.6.3, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[10.0.1, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[10.0.1, )"))
                    ]),
                    new DependencySet("net10.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[10.0.1, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[10.0.1, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.6.3, )"))
                    ])
                ),
            }
        ];

        public static IReadOnlyCollection<PackageVersionWithDependencySets> CreateDependencySetsMicrosoft_Extensions_Options() =>
        [
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.0"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.0, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.0, )")),
                        new PackageDependencyInfo("System.ValueTuple", CreatePackageVersion("[4.5.0, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.0, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.0, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.0, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.0, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.0, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.0, )")),
                        new PackageDependencyInfo("System.ComponentModel.Annotations", CreatePackageVersion("[5.0.0, )"))
                    ]),
                    new DependencySet("netstandard2.1", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.0, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.0, )")),
                        new PackageDependencyInfo("System.ComponentModel.Annotations", CreatePackageVersion("[5.0.0, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.1"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.1, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.1, )")),
                        new PackageDependencyInfo("System.ValueTuple", CreatePackageVersion("[4.5.0, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.1, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.1, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.1, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.1, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.1, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.1, )")),
                        new PackageDependencyInfo("System.ComponentModel.Annotations", CreatePackageVersion("[5.0.0, )"))
                    ]),
                    new DependencySet("netstandard2.1", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.1, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.1, )")),
                        new PackageDependencyInfo("System.ComponentModel.Annotations", CreatePackageVersion("[5.0.0, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.2"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.2, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.2, )")),
                        new PackageDependencyInfo("System.ValueTuple", CreatePackageVersion("[4.5.0, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.2, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.2, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.2, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.2, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.2, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.2, )")),
                        new PackageDependencyInfo("System.ComponentModel.Annotations", CreatePackageVersion("[5.0.0, )"))
                    ]),
                    new DependencySet("netstandard2.1", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.2, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.2, )")),
                        new PackageDependencyInfo("System.ComponentModel.Annotations", CreatePackageVersion("[5.0.0, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.3"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.3, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.3, )")),
                        new PackageDependencyInfo("System.ValueTuple", CreatePackageVersion("[4.5.0, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.3, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.3, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.3, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.3, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.3, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.3, )")),
                        new PackageDependencyInfo("System.ComponentModel.Annotations", CreatePackageVersion("[5.0.0, )"))
                    ]),
                    new DependencySet("netstandard2.1", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.3, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.3, )")),
                        new PackageDependencyInfo("System.ComponentModel.Annotations", CreatePackageVersion("[5.0.0, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.4"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.4, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.4, )")),
                        new PackageDependencyInfo("System.ValueTuple", CreatePackageVersion("[4.5.0, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.4, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.4, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.4, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.4, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.4, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.4, )")),
                        new PackageDependencyInfo("System.ComponentModel.Annotations", CreatePackageVersion("[5.0.0, )"))
                    ]),
                    new DependencySet("netstandard2.1", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.4, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.4, )")),
                        new PackageDependencyInfo("System.ComponentModel.Annotations", CreatePackageVersion("[5.0.0, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.5"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.5, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.5, )")),
                        new PackageDependencyInfo("System.ValueTuple", CreatePackageVersion("[4.5.0, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.5, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.5, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.5, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.5, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.5, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.5, )")),
                        new PackageDependencyInfo("System.ComponentModel.Annotations", CreatePackageVersion("[5.0.0, )"))
                    ]),
                    new DependencySet("netstandard2.1", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.5, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.5, )")),
                        new PackageDependencyInfo("System.ComponentModel.Annotations", CreatePackageVersion("[5.0.0, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.6"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.6, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.6, )")),
                        new PackageDependencyInfo("System.ValueTuple", CreatePackageVersion("[4.5.0, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.6, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.6, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.6, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.6, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.6, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.6, )")),
                        new PackageDependencyInfo("System.ComponentModel.Annotations", CreatePackageVersion("[5.0.0, )"))
                    ]),
                    new DependencySet("netstandard2.1", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.6, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.6, )")),
                        new PackageDependencyInfo("System.ComponentModel.Annotations", CreatePackageVersion("[5.0.0, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.7"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.7, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.7, )")),
                        new PackageDependencyInfo("System.ValueTuple", CreatePackageVersion("[4.5.0, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.7, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.7, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.7, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.7, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.7, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.7, )")),
                        new PackageDependencyInfo("System.ComponentModel.Annotations", CreatePackageVersion("[5.0.0, )"))
                    ]),
                    new DependencySet("netstandard2.1", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.7, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.7, )")),
                        new PackageDependencyInfo("System.ComponentModel.Annotations", CreatePackageVersion("[5.0.0, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.8"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.8, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.8, )")),
                        new PackageDependencyInfo("System.ValueTuple", CreatePackageVersion("[4.5.0, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.8, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.8, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.8, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.8, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.8, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.8, )")),
                        new PackageDependencyInfo("System.ComponentModel.Annotations", CreatePackageVersion("[5.0.0, )"))
                    ]),
                    new DependencySet("netstandard2.1", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.8, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.8, )")),
                        new PackageDependencyInfo("System.ComponentModel.Annotations", CreatePackageVersion("[5.0.0, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.9"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.9, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.9, )")),
                        new PackageDependencyInfo("System.ValueTuple", CreatePackageVersion("[4.5.0, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.9, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.9, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.9, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.9, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.9, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.9, )")),
                        new PackageDependencyInfo("System.ComponentModel.Annotations", CreatePackageVersion("[5.0.0, )"))
                    ]),
                    new DependencySet("netstandard2.1", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.9, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.9, )")),
                        new PackageDependencyInfo("System.ComponentModel.Annotations", CreatePackageVersion("[5.0.0, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.10"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.10, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.10, )")),
                        new PackageDependencyInfo("System.ValueTuple", CreatePackageVersion("[4.5.0, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.10, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.10, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.10, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.10, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.10, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.10, )")),
                        new PackageDependencyInfo("System.ComponentModel.Annotations", CreatePackageVersion("[5.0.0, )"))
                    ]),
                    new DependencySet("netstandard2.1", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.10, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.10, )")),
                        new PackageDependencyInfo("System.ComponentModel.Annotations", CreatePackageVersion("[5.0.0, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.11"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.11, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.11, )")),
                        new PackageDependencyInfo("System.ValueTuple", CreatePackageVersion("[4.5.0, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.11, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.11, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.11, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.11, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.11, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.11, )")),
                        new PackageDependencyInfo("System.ComponentModel.Annotations", CreatePackageVersion("[5.0.0, )"))
                    ]),
                    new DependencySet("netstandard2.1", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[9.0.11, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[9.0.11, )")),
                        new PackageDependencyInfo("System.ComponentModel.Annotations", CreatePackageVersion("[5.0.0, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("10.0.0"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[10.0.0, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[10.0.0, )")),
                        new PackageDependencyInfo("System.ValueTuple", CreatePackageVersion("[4.6.1, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[10.0.0, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[10.0.0, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[10.0.0, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[10.0.0, )"))
                    ]),
                    new DependencySet("net10.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[10.0.0, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[10.0.0, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[10.0.0, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[10.0.0, )")),
                        new PackageDependencyInfo("System.ComponentModel.Annotations", CreatePackageVersion("[5.0.0, )"))
                    ]),
                    new DependencySet("netstandard2.1", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[10.0.0, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[10.0.0, )")),
                        new PackageDependencyInfo("System.ComponentModel.Annotations", CreatePackageVersion("[5.0.0, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("10.0.1"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[10.0.1, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[10.0.1, )")),
                        new PackageDependencyInfo("System.ValueTuple", CreatePackageVersion("[4.6.1, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[10.0.1, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[10.0.1, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[10.0.1, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[10.0.1, )"))
                    ]),
                    new DependencySet("net10.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[10.0.1, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[10.0.1, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[10.0.1, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[10.0.1, )")),
                        new PackageDependencyInfo("System.ComponentModel.Annotations", CreatePackageVersion("[5.0.0, )"))
                    ]),
                    new DependencySet("netstandard2.1", [
                        new PackageDependencyInfo("Microsoft.Extensions.DependencyInjection.Abstractions", CreatePackageVersion("[10.0.1, )")),
                        new PackageDependencyInfo("Microsoft.Extensions.Primitives", CreatePackageVersion("[10.0.1, )")),
                        new PackageDependencyInfo("System.ComponentModel.Annotations", CreatePackageVersion("[5.0.0, )"))
                    ])
                ),
            }
        ];

        public static IReadOnlyCollection<PackageVersionWithDependencySets> CreateDependencySetsNSubstitute_Analyzers_CSharp() =>
        [
        ];

        public static IReadOnlyCollection<PackageVersionWithDependencySets> CreateDependencySetszeroql_cli() =>
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
        ];

        public static IReadOnlyCollection<PackageVersionWithDependencySets> CreateDependencySetsMicrosoft_Bcl_AsyncInterfaces() =>
        [
            new PackageVersionWithDependencySets(CreatePackageVersion("10.0.0"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.6.3, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.6.3, )"))
                    ]),
                    new DependencySet("netstandard2.1", [
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("10.0.1"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.6.3, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.6.3, )"))
                    ]),
                    new DependencySet("netstandard2.1", [
                    ])
                ),
            }
        ];

        public static IReadOnlyCollection<PackageVersionWithDependencySets> CreateDependencySetsSystem_Text_Json() =>
        [
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.0"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.0, )")),
                        new PackageDependencyInfo("System.IO.Pipelines", CreatePackageVersion("[9.0.0, )")),
                        new PackageDependencyInfo("System.Text.Encodings.Web", CreatePackageVersion("[9.0.0, )")),
                        new PackageDependencyInfo("System.Buffers", CreatePackageVersion("[4.5.1, )")),
                        new PackageDependencyInfo("System.Memory", CreatePackageVersion("[4.5.5, )")),
                        new PackageDependencyInfo("System.Runtime.CompilerServices.Unsafe", CreatePackageVersion("[6.0.0, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )")),
                        new PackageDependencyInfo("System.ValueTuple", CreatePackageVersion("[4.5.0, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("System.IO.Pipelines", CreatePackageVersion("[9.0.0, )")),
                        new PackageDependencyInfo("System.Text.Encodings.Web", CreatePackageVersion("[9.0.0, )"))
                    ]),
                    new DependencySet("net9.0", [
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.0, )")),
                        new PackageDependencyInfo("System.IO.Pipelines", CreatePackageVersion("[9.0.0, )")),
                        new PackageDependencyInfo("System.Text.Encodings.Web", CreatePackageVersion("[9.0.0, )")),
                        new PackageDependencyInfo("System.Buffers", CreatePackageVersion("[4.5.1, )")),
                        new PackageDependencyInfo("System.Memory", CreatePackageVersion("[4.5.5, )")),
                        new PackageDependencyInfo("System.Runtime.CompilerServices.Unsafe", CreatePackageVersion("[6.0.0, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.1"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.1, )")),
                        new PackageDependencyInfo("System.IO.Pipelines", CreatePackageVersion("[9.0.1, )")),
                        new PackageDependencyInfo("System.Text.Encodings.Web", CreatePackageVersion("[9.0.1, )")),
                        new PackageDependencyInfo("System.Buffers", CreatePackageVersion("[4.5.1, )")),
                        new PackageDependencyInfo("System.Memory", CreatePackageVersion("[4.5.5, )")),
                        new PackageDependencyInfo("System.Runtime.CompilerServices.Unsafe", CreatePackageVersion("[6.0.0, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )")),
                        new PackageDependencyInfo("System.ValueTuple", CreatePackageVersion("[4.5.0, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("System.IO.Pipelines", CreatePackageVersion("[9.0.1, )")),
                        new PackageDependencyInfo("System.Text.Encodings.Web", CreatePackageVersion("[9.0.1, )"))
                    ]),
                    new DependencySet("net9.0", [
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.1, )")),
                        new PackageDependencyInfo("System.IO.Pipelines", CreatePackageVersion("[9.0.1, )")),
                        new PackageDependencyInfo("System.Text.Encodings.Web", CreatePackageVersion("[9.0.1, )")),
                        new PackageDependencyInfo("System.Buffers", CreatePackageVersion("[4.5.1, )")),
                        new PackageDependencyInfo("System.Memory", CreatePackageVersion("[4.5.5, )")),
                        new PackageDependencyInfo("System.Runtime.CompilerServices.Unsafe", CreatePackageVersion("[6.0.0, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.2"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.2, )")),
                        new PackageDependencyInfo("System.IO.Pipelines", CreatePackageVersion("[9.0.2, )")),
                        new PackageDependencyInfo("System.Text.Encodings.Web", CreatePackageVersion("[9.0.2, )")),
                        new PackageDependencyInfo("System.Buffers", CreatePackageVersion("[4.5.1, )")),
                        new PackageDependencyInfo("System.Memory", CreatePackageVersion("[4.5.5, )")),
                        new PackageDependencyInfo("System.Runtime.CompilerServices.Unsafe", CreatePackageVersion("[6.0.0, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )")),
                        new PackageDependencyInfo("System.ValueTuple", CreatePackageVersion("[4.5.0, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("System.IO.Pipelines", CreatePackageVersion("[9.0.2, )")),
                        new PackageDependencyInfo("System.Text.Encodings.Web", CreatePackageVersion("[9.0.2, )"))
                    ]),
                    new DependencySet("net9.0", [
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.2, )")),
                        new PackageDependencyInfo("System.IO.Pipelines", CreatePackageVersion("[9.0.2, )")),
                        new PackageDependencyInfo("System.Text.Encodings.Web", CreatePackageVersion("[9.0.2, )")),
                        new PackageDependencyInfo("System.Buffers", CreatePackageVersion("[4.5.1, )")),
                        new PackageDependencyInfo("System.Memory", CreatePackageVersion("[4.5.5, )")),
                        new PackageDependencyInfo("System.Runtime.CompilerServices.Unsafe", CreatePackageVersion("[6.0.0, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.3"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.3, )")),
                        new PackageDependencyInfo("System.IO.Pipelines", CreatePackageVersion("[9.0.3, )")),
                        new PackageDependencyInfo("System.Text.Encodings.Web", CreatePackageVersion("[9.0.3, )")),
                        new PackageDependencyInfo("System.Buffers", CreatePackageVersion("[4.5.1, )")),
                        new PackageDependencyInfo("System.Memory", CreatePackageVersion("[4.5.5, )")),
                        new PackageDependencyInfo("System.Runtime.CompilerServices.Unsafe", CreatePackageVersion("[6.0.0, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )")),
                        new PackageDependencyInfo("System.ValueTuple", CreatePackageVersion("[4.5.0, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("System.IO.Pipelines", CreatePackageVersion("[9.0.3, )")),
                        new PackageDependencyInfo("System.Text.Encodings.Web", CreatePackageVersion("[9.0.3, )"))
                    ]),
                    new DependencySet("net9.0", [
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.3, )")),
                        new PackageDependencyInfo("System.IO.Pipelines", CreatePackageVersion("[9.0.3, )")),
                        new PackageDependencyInfo("System.Text.Encodings.Web", CreatePackageVersion("[9.0.3, )")),
                        new PackageDependencyInfo("System.Buffers", CreatePackageVersion("[4.5.1, )")),
                        new PackageDependencyInfo("System.Memory", CreatePackageVersion("[4.5.5, )")),
                        new PackageDependencyInfo("System.Runtime.CompilerServices.Unsafe", CreatePackageVersion("[6.0.0, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.4"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.4, )")),
                        new PackageDependencyInfo("System.IO.Pipelines", CreatePackageVersion("[9.0.4, )")),
                        new PackageDependencyInfo("System.Text.Encodings.Web", CreatePackageVersion("[9.0.4, )")),
                        new PackageDependencyInfo("System.Buffers", CreatePackageVersion("[4.5.1, )")),
                        new PackageDependencyInfo("System.Memory", CreatePackageVersion("[4.5.5, )")),
                        new PackageDependencyInfo("System.Runtime.CompilerServices.Unsafe", CreatePackageVersion("[6.0.0, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )")),
                        new PackageDependencyInfo("System.ValueTuple", CreatePackageVersion("[4.5.0, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("System.IO.Pipelines", CreatePackageVersion("[9.0.4, )")),
                        new PackageDependencyInfo("System.Text.Encodings.Web", CreatePackageVersion("[9.0.4, )"))
                    ]),
                    new DependencySet("net9.0", [
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.4, )")),
                        new PackageDependencyInfo("System.IO.Pipelines", CreatePackageVersion("[9.0.4, )")),
                        new PackageDependencyInfo("System.Text.Encodings.Web", CreatePackageVersion("[9.0.4, )")),
                        new PackageDependencyInfo("System.Buffers", CreatePackageVersion("[4.5.1, )")),
                        new PackageDependencyInfo("System.Memory", CreatePackageVersion("[4.5.5, )")),
                        new PackageDependencyInfo("System.Runtime.CompilerServices.Unsafe", CreatePackageVersion("[6.0.0, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.5"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.5, )")),
                        new PackageDependencyInfo("System.IO.Pipelines", CreatePackageVersion("[9.0.5, )")),
                        new PackageDependencyInfo("System.Text.Encodings.Web", CreatePackageVersion("[9.0.5, )")),
                        new PackageDependencyInfo("System.Buffers", CreatePackageVersion("[4.5.1, )")),
                        new PackageDependencyInfo("System.Memory", CreatePackageVersion("[4.5.5, )")),
                        new PackageDependencyInfo("System.Runtime.CompilerServices.Unsafe", CreatePackageVersion("[6.0.0, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )")),
                        new PackageDependencyInfo("System.ValueTuple", CreatePackageVersion("[4.5.0, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("System.IO.Pipelines", CreatePackageVersion("[9.0.5, )")),
                        new PackageDependencyInfo("System.Text.Encodings.Web", CreatePackageVersion("[9.0.5, )"))
                    ]),
                    new DependencySet("net9.0", [
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.5, )")),
                        new PackageDependencyInfo("System.IO.Pipelines", CreatePackageVersion("[9.0.5, )")),
                        new PackageDependencyInfo("System.Text.Encodings.Web", CreatePackageVersion("[9.0.5, )")),
                        new PackageDependencyInfo("System.Buffers", CreatePackageVersion("[4.5.1, )")),
                        new PackageDependencyInfo("System.Memory", CreatePackageVersion("[4.5.5, )")),
                        new PackageDependencyInfo("System.Runtime.CompilerServices.Unsafe", CreatePackageVersion("[6.0.0, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.6"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.6, )")),
                        new PackageDependencyInfo("System.IO.Pipelines", CreatePackageVersion("[9.0.6, )")),
                        new PackageDependencyInfo("System.Text.Encodings.Web", CreatePackageVersion("[9.0.6, )")),
                        new PackageDependencyInfo("System.Buffers", CreatePackageVersion("[4.5.1, )")),
                        new PackageDependencyInfo("System.Memory", CreatePackageVersion("[4.5.5, )")),
                        new PackageDependencyInfo("System.Runtime.CompilerServices.Unsafe", CreatePackageVersion("[6.0.0, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )")),
                        new PackageDependencyInfo("System.ValueTuple", CreatePackageVersion("[4.5.0, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("System.IO.Pipelines", CreatePackageVersion("[9.0.6, )")),
                        new PackageDependencyInfo("System.Text.Encodings.Web", CreatePackageVersion("[9.0.6, )"))
                    ]),
                    new DependencySet("net9.0", [
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.6, )")),
                        new PackageDependencyInfo("System.IO.Pipelines", CreatePackageVersion("[9.0.6, )")),
                        new PackageDependencyInfo("System.Text.Encodings.Web", CreatePackageVersion("[9.0.6, )")),
                        new PackageDependencyInfo("System.Buffers", CreatePackageVersion("[4.5.1, )")),
                        new PackageDependencyInfo("System.Memory", CreatePackageVersion("[4.5.5, )")),
                        new PackageDependencyInfo("System.Runtime.CompilerServices.Unsafe", CreatePackageVersion("[6.0.0, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.7"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.7, )")),
                        new PackageDependencyInfo("System.IO.Pipelines", CreatePackageVersion("[9.0.7, )")),
                        new PackageDependencyInfo("System.Text.Encodings.Web", CreatePackageVersion("[9.0.7, )")),
                        new PackageDependencyInfo("System.Buffers", CreatePackageVersion("[4.5.1, )")),
                        new PackageDependencyInfo("System.Memory", CreatePackageVersion("[4.5.5, )")),
                        new PackageDependencyInfo("System.Runtime.CompilerServices.Unsafe", CreatePackageVersion("[6.0.0, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )")),
                        new PackageDependencyInfo("System.ValueTuple", CreatePackageVersion("[4.5.0, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("System.IO.Pipelines", CreatePackageVersion("[9.0.7, )")),
                        new PackageDependencyInfo("System.Text.Encodings.Web", CreatePackageVersion("[9.0.7, )"))
                    ]),
                    new DependencySet("net9.0", [
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.7, )")),
                        new PackageDependencyInfo("System.IO.Pipelines", CreatePackageVersion("[9.0.7, )")),
                        new PackageDependencyInfo("System.Text.Encodings.Web", CreatePackageVersion("[9.0.7, )")),
                        new PackageDependencyInfo("System.Buffers", CreatePackageVersion("[4.5.1, )")),
                        new PackageDependencyInfo("System.Memory", CreatePackageVersion("[4.5.5, )")),
                        new PackageDependencyInfo("System.Runtime.CompilerServices.Unsafe", CreatePackageVersion("[6.0.0, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.8"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.8, )")),
                        new PackageDependencyInfo("System.IO.Pipelines", CreatePackageVersion("[9.0.8, )")),
                        new PackageDependencyInfo("System.Text.Encodings.Web", CreatePackageVersion("[9.0.8, )")),
                        new PackageDependencyInfo("System.Buffers", CreatePackageVersion("[4.5.1, )")),
                        new PackageDependencyInfo("System.Memory", CreatePackageVersion("[4.5.5, )")),
                        new PackageDependencyInfo("System.Runtime.CompilerServices.Unsafe", CreatePackageVersion("[6.0.0, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )")),
                        new PackageDependencyInfo("System.ValueTuple", CreatePackageVersion("[4.5.0, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("System.IO.Pipelines", CreatePackageVersion("[9.0.8, )")),
                        new PackageDependencyInfo("System.Text.Encodings.Web", CreatePackageVersion("[9.0.8, )"))
                    ]),
                    new DependencySet("net9.0", [
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.8, )")),
                        new PackageDependencyInfo("System.IO.Pipelines", CreatePackageVersion("[9.0.8, )")),
                        new PackageDependencyInfo("System.Text.Encodings.Web", CreatePackageVersion("[9.0.8, )")),
                        new PackageDependencyInfo("System.Buffers", CreatePackageVersion("[4.5.1, )")),
                        new PackageDependencyInfo("System.Memory", CreatePackageVersion("[4.5.5, )")),
                        new PackageDependencyInfo("System.Runtime.CompilerServices.Unsafe", CreatePackageVersion("[6.0.0, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.9"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.9, )")),
                        new PackageDependencyInfo("System.IO.Pipelines", CreatePackageVersion("[9.0.9, )")),
                        new PackageDependencyInfo("System.Text.Encodings.Web", CreatePackageVersion("[9.0.9, )")),
                        new PackageDependencyInfo("System.Buffers", CreatePackageVersion("[4.5.1, )")),
                        new PackageDependencyInfo("System.Memory", CreatePackageVersion("[4.5.5, )")),
                        new PackageDependencyInfo("System.Runtime.CompilerServices.Unsafe", CreatePackageVersion("[6.0.0, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )")),
                        new PackageDependencyInfo("System.ValueTuple", CreatePackageVersion("[4.5.0, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("System.IO.Pipelines", CreatePackageVersion("[9.0.9, )")),
                        new PackageDependencyInfo("System.Text.Encodings.Web", CreatePackageVersion("[9.0.9, )"))
                    ]),
                    new DependencySet("net9.0", [
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.9, )")),
                        new PackageDependencyInfo("System.IO.Pipelines", CreatePackageVersion("[9.0.9, )")),
                        new PackageDependencyInfo("System.Text.Encodings.Web", CreatePackageVersion("[9.0.9, )")),
                        new PackageDependencyInfo("System.Buffers", CreatePackageVersion("[4.5.1, )")),
                        new PackageDependencyInfo("System.Memory", CreatePackageVersion("[4.5.5, )")),
                        new PackageDependencyInfo("System.Runtime.CompilerServices.Unsafe", CreatePackageVersion("[6.0.0, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.10"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.10, )")),
                        new PackageDependencyInfo("System.IO.Pipelines", CreatePackageVersion("[9.0.10, )")),
                        new PackageDependencyInfo("System.Text.Encodings.Web", CreatePackageVersion("[9.0.10, )")),
                        new PackageDependencyInfo("System.Buffers", CreatePackageVersion("[4.5.1, )")),
                        new PackageDependencyInfo("System.Memory", CreatePackageVersion("[4.5.5, )")),
                        new PackageDependencyInfo("System.Runtime.CompilerServices.Unsafe", CreatePackageVersion("[6.0.0, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )")),
                        new PackageDependencyInfo("System.ValueTuple", CreatePackageVersion("[4.5.0, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("System.IO.Pipelines", CreatePackageVersion("[9.0.10, )")),
                        new PackageDependencyInfo("System.Text.Encodings.Web", CreatePackageVersion("[9.0.10, )"))
                    ]),
                    new DependencySet("net9.0", [
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.10, )")),
                        new PackageDependencyInfo("System.IO.Pipelines", CreatePackageVersion("[9.0.10, )")),
                        new PackageDependencyInfo("System.Text.Encodings.Web", CreatePackageVersion("[9.0.10, )")),
                        new PackageDependencyInfo("System.Buffers", CreatePackageVersion("[4.5.1, )")),
                        new PackageDependencyInfo("System.Memory", CreatePackageVersion("[4.5.5, )")),
                        new PackageDependencyInfo("System.Runtime.CompilerServices.Unsafe", CreatePackageVersion("[6.0.0, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.11"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.11, )")),
                        new PackageDependencyInfo("System.IO.Pipelines", CreatePackageVersion("[9.0.11, )")),
                        new PackageDependencyInfo("System.Text.Encodings.Web", CreatePackageVersion("[9.0.11, )")),
                        new PackageDependencyInfo("System.Buffers", CreatePackageVersion("[4.5.1, )")),
                        new PackageDependencyInfo("System.Memory", CreatePackageVersion("[4.5.5, )")),
                        new PackageDependencyInfo("System.Runtime.CompilerServices.Unsafe", CreatePackageVersion("[6.0.0, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )")),
                        new PackageDependencyInfo("System.ValueTuple", CreatePackageVersion("[4.5.0, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("System.IO.Pipelines", CreatePackageVersion("[9.0.11, )")),
                        new PackageDependencyInfo("System.Text.Encodings.Web", CreatePackageVersion("[9.0.11, )"))
                    ]),
                    new DependencySet("net9.0", [
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.11, )")),
                        new PackageDependencyInfo("System.IO.Pipelines", CreatePackageVersion("[9.0.11, )")),
                        new PackageDependencyInfo("System.Text.Encodings.Web", CreatePackageVersion("[9.0.11, )")),
                        new PackageDependencyInfo("System.Buffers", CreatePackageVersion("[4.5.1, )")),
                        new PackageDependencyInfo("System.Memory", CreatePackageVersion("[4.5.5, )")),
                        new PackageDependencyInfo("System.Runtime.CompilerServices.Unsafe", CreatePackageVersion("[6.0.0, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("10.0.0"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[10.0.0, )")),
                        new PackageDependencyInfo("System.IO.Pipelines", CreatePackageVersion("[10.0.0, )")),
                        new PackageDependencyInfo("System.Text.Encodings.Web", CreatePackageVersion("[10.0.0, )")),
                        new PackageDependencyInfo("System.Buffers", CreatePackageVersion("[4.6.1, )")),
                        new PackageDependencyInfo("System.Memory", CreatePackageVersion("[4.6.3, )")),
                        new PackageDependencyInfo("System.Runtime.CompilerServices.Unsafe", CreatePackageVersion("[6.1.2, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.6.3, )")),
                        new PackageDependencyInfo("System.ValueTuple", CreatePackageVersion("[4.6.1, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("System.IO.Pipelines", CreatePackageVersion("[10.0.0, )")),
                        new PackageDependencyInfo("System.Text.Encodings.Web", CreatePackageVersion("[10.0.0, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("System.IO.Pipelines", CreatePackageVersion("[10.0.0, )")),
                        new PackageDependencyInfo("System.Text.Encodings.Web", CreatePackageVersion("[10.0.0, )"))
                    ]),
                    new DependencySet("net10.0", [
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[10.0.0, )")),
                        new PackageDependencyInfo("System.IO.Pipelines", CreatePackageVersion("[10.0.0, )")),
                        new PackageDependencyInfo("System.Text.Encodings.Web", CreatePackageVersion("[10.0.0, )")),
                        new PackageDependencyInfo("System.Buffers", CreatePackageVersion("[4.6.1, )")),
                        new PackageDependencyInfo("System.Memory", CreatePackageVersion("[4.6.3, )")),
                        new PackageDependencyInfo("System.Runtime.CompilerServices.Unsafe", CreatePackageVersion("[6.1.2, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.6.3, )"))
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("10.0.1"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[10.0.1, )")),
                        new PackageDependencyInfo("System.IO.Pipelines", CreatePackageVersion("[10.0.1, )")),
                        new PackageDependencyInfo("System.Text.Encodings.Web", CreatePackageVersion("[10.0.1, )")),
                        new PackageDependencyInfo("System.Buffers", CreatePackageVersion("[4.6.1, )")),
                        new PackageDependencyInfo("System.Memory", CreatePackageVersion("[4.6.3, )")),
                        new PackageDependencyInfo("System.Runtime.CompilerServices.Unsafe", CreatePackageVersion("[6.1.2, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.6.3, )")),
                        new PackageDependencyInfo("System.ValueTuple", CreatePackageVersion("[4.6.1, )"))
                    ]),
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("System.IO.Pipelines", CreatePackageVersion("[10.0.1, )")),
                        new PackageDependencyInfo("System.Text.Encodings.Web", CreatePackageVersion("[10.0.1, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("System.IO.Pipelines", CreatePackageVersion("[10.0.1, )")),
                        new PackageDependencyInfo("System.Text.Encodings.Web", CreatePackageVersion("[10.0.1, )"))
                    ]),
                    new DependencySet("net10.0", [
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[10.0.1, )")),
                        new PackageDependencyInfo("System.IO.Pipelines", CreatePackageVersion("[10.0.1, )")),
                        new PackageDependencyInfo("System.Text.Encodings.Web", CreatePackageVersion("[10.0.1, )")),
                        new PackageDependencyInfo("System.Buffers", CreatePackageVersion("[4.6.1, )")),
                        new PackageDependencyInfo("System.Memory", CreatePackageVersion("[4.6.3, )")),
                        new PackageDependencyInfo("System.Runtime.CompilerServices.Unsafe", CreatePackageVersion("[6.1.2, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.6.3, )"))
                    ])
                ),
            }
        ];

        public static IReadOnlyCollection<PackageVersionWithDependencySets> CreateDependencySetsMicrosoft_Extensions_DependencyInjection_Abstractions() =>
        [
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.0"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.0, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("net8.0", [
                    ]),
                    new DependencySet("net9.0", [
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.0, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("netstandard2.1", [
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.1"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.1, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("net8.0", [
                    ]),
                    new DependencySet("net9.0", [
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.1, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("netstandard2.1", [
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.2"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.2, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("net8.0", [
                    ]),
                    new DependencySet("net9.0", [
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.2, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("netstandard2.1", [
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.3"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.3, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("net8.0", [
                    ]),
                    new DependencySet("net9.0", [
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.3, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("netstandard2.1", [
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.4"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.4, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("net8.0", [
                    ]),
                    new DependencySet("net9.0", [
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.4, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("netstandard2.1", [
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.5"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.5, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("net8.0", [
                    ]),
                    new DependencySet("net9.0", [
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.5, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("netstandard2.1", [
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.6"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.6, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("net8.0", [
                    ]),
                    new DependencySet("net9.0", [
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.6, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("netstandard2.1", [
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.7"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.7, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("net8.0", [
                    ]),
                    new DependencySet("net9.0", [
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.7, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("netstandard2.1", [
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.8"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.8, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("net8.0", [
                    ]),
                    new DependencySet("net9.0", [
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.8, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("netstandard2.1", [
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.9"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.9, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("net8.0", [
                    ]),
                    new DependencySet("net9.0", [
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.9, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("netstandard2.1", [
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.10"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.10, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("net8.0", [
                    ]),
                    new DependencySet("net9.0", [
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.10, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("netstandard2.1", [
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("9.0.11"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.11, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("net8.0", [
                    ]),
                    new DependencySet("net9.0", [
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[9.0.11, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.5.4, )"))
                    ]),
                    new DependencySet("netstandard2.1", [
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("10.0.0"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[10.0.0, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.6.3, )"))
                    ]),
                    new DependencySet("net8.0", [
                    ]),
                    new DependencySet("net9.0", [
                    ]),
                    new DependencySet("net10.0", [
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[10.0.0, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.6.3, )"))
                    ]),
                    new DependencySet("netstandard2.1", [
                    ])
                ),
            },
            new PackageVersionWithDependencySets(CreatePackageVersion("10.0.1"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net462", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[10.0.1, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.6.3, )"))
                    ]),
                    new DependencySet("net8.0", [
                    ]),
                    new DependencySet("net9.0", [
                    ]),
                    new DependencySet("net10.0", [
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Bcl.AsyncInterfaces", CreatePackageVersion("[10.0.1, )")),
                        new PackageDependencyInfo("System.Threading.Tasks.Extensions", CreatePackageVersion("[4.6.3, )"))
                    ]),
                    new DependencySet("netstandard2.1", [
                    ])
                ),
            }
        ];

        public static IReadOnlyCollection<PackageVersionWithDependencySets> CreateDependencySetsMeziantou_Extensions_Logging_Xunit_v3() =>
        [
            new PackageVersionWithDependencySets(CreatePackageVersion("1.1.20"))
            {
                DependencySets = CreateDependencySets(
                    new DependencySet("net8.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[10.0.1, )")),
                        new PackageDependencyInfo("xunit.v3.extensibility.core", CreatePackageVersion("[3.2.1, )"))
                    ]),
                    new DependencySet("net9.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[10.0.1, )")),
                        new PackageDependencyInfo("xunit.v3.extensibility.core", CreatePackageVersion("[3.2.1, )"))
                    ]),
                    new DependencySet("net10.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[10.0.1, )")),
                        new PackageDependencyInfo("xunit.v3.extensibility.core", CreatePackageVersion("[3.2.1, )"))
                    ]),
                    new DependencySet("netstandard2.0", [
                        new PackageDependencyInfo("Microsoft.Extensions.Logging.Abstractions", CreatePackageVersion("[10.0.1, )")),
                        new PackageDependencyInfo("xunit.v3.extensibility.core", CreatePackageVersion("[3.2.1, )"))
                    ])
                ),
            }
        ];

        public static IReadOnlyCollection<PackageVersionWithDependencySets> CreateDependencySetsCastle_Core() =>
        [
        ];

        public static IReadOnlyCollection<PackageVersionWithDependencySets> CreateDependencySetsxunit_v3_mtp_v2() =>
        [
        ];

        public static IReadOnlyCollection<PackageVersionWithDependencySets> CreateDependencySetsMicrosoft_Testing_Extensions_CodeCoverage() =>
        [
        ];

        public static IReadOnlyCollection<PackageVersionWithDependencySets> CreateDependencySetsmicrosoft_openapi_kiota() =>
        [
        ];
    }
}
