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
