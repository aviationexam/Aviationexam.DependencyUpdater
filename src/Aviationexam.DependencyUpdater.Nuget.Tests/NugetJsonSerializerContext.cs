using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Aviationexam.DependencyUpdater.Nuget.Tests;

[JsonSerializable(typeof(IEnumerable<IPackageSearchMetadata>))]
[JsonSerializable(typeof(IEnumerable<PackageSearchMetadataRegistration>))]
[JsonSourceGenerationOptions(WriteIndented = true)]
public partial class NugetJsonSerializerContext : JsonSerializerContext;
