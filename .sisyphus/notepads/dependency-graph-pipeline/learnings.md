# Learnings

## 2026-04-01 Session Start
- Project uses sealed record for DTOs, sealed class with primary constructors for services
- ILogger<T> (generic) required — non-generic breaks IoC
- CancellationToken always last parameter on async methods
- Tests use TestContext.Current.CancellationToken, never CancellationToken.None
- Warnings as errors in Release configuration
- File-scoped namespaces: `namespace X;`
- IFileSystem abstraction for all file operations
- Dependency graph DTOs should stay sealed records; node equality must ignore metadata availability
- NugetCsprojParser.ParseProjectReferences() follows same pattern as Parse(): file open via IFileSystem, XDocument.Load, TFM parsing, condition handling via ConditionalTargetFrameworkResolver
- ProjectReference Include attribute contains relative path; use Path.GetFileNameWithoutExtension() for project name
- ConditionalTargetFrameworkResolver.Resolve() works for both PackageReference and ProjectReference conditions (accepts any identifier string for logging)
- xUnit v3 with Microsoft.Testing.Platform uses --filter-class/--filter-method instead of --filter
- DependencyGraphColorizer should accumulate links in three phases: direct project refs, transitive project-reference inheritance (with full project chain), then transitive package-edge traversal from each inherited/direct seed
- Rebuilding colored graphs should preserve original node metadata + edges via DependencyGraphBuilder and only add ProjectDependencyLink entries
- DependencyGraphConstructor pattern mirrors DependencyAnalyzer: map sources via NugetUpdaterContextExtensions, seed queue from direct dependencies, then BFS transitive package dependencies
- For package dependency traversal, use PackageVersionWithDependencySets.DependencySets with source preference order Default -> Fallback and create edge target TFM from DependencySet.TargetFramework
- When FetchDependencyVersionsAsync has no matching current version metadata, keep node in graph marked as metadata unavailable and log warning

## 2026-04-01 Pipeline Integration Tests
- DependencyGraphPipeline end-to-end tests should mock INugetVersionFetcher and keep real NugetCsprojParser + DependencyGraphConstructor + DependencyGraphColorizer to validate orchestration
- IFileSystem mock must return fresh stream instances for csproj reads because pipeline parses each file twice (PackageReference and ProjectReference passes)
- NugetFinder.GetAllNugetFiles calls EnumerateFiles for four patterns (*.csproj, Directory.Packages.props, Nuget.Config, dotnet-tools.json), so integration tests must setup all of them
- Package metadata returned by mocks must be PackageSearchMetadataRegistration (not arbitrary IPackageSearchMetadata) because mapper enforces registration type
