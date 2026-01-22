# AGENTS.md

This file provides guidance for AI coding agents working in the Aviationexam.DependencyUpdater codebase.

## Build, Lint, and Test Commands

### Build
```bash
# Restore dependencies
dotnet restore --nologo

# Build solution
dotnet build --no-restore --nologo --configuration Release
```

### Format and Lint
```bash
# Check code formatting (CI mode)
dotnet format --no-restore --verify-no-changes -v diag

# Apply formatting
dotnet format
```

### Run Tests
```bash
# Run all tests
dotnet test --no-build --configuration Release

# Run a specific test by fully qualified name
dotnet test --no-build --configuration Release --filter "FullyQualifiedName~TestMethodName"

# Run tests in a specific test project
dotnet test src/Aviationexam.DependencyUpdater.Nuget.Tests/Aviationexam.DependencyUpdater.Nuget.Tests.csproj --no-build --configuration Release

# Run tests with TRX report output
dotnet test --no-build --configuration Release --results-directory TestResults --report-trx
```

### Run the Tool Locally
```bash
# Azure DevOps
dotnet run --project src/Aviationexam.DependencyUpdater/Aviationexam.DependencyUpdater.csproj -- \
  --directory "/path/to/repo" --git-password "<token>" \
  AzureDevOps --organization "<org>" --project "<project-id>" --repository "<repo-id>" \
  --pat "<pat>" --account-id "<account-id>" --nuget-project "<nuget-project-id>" \
  --nuget-feed-id "<feed-id>" --nuget-service-host "<service-host>" \
  --access-token-resource-id "499b84ac-1321-427f-aa17-267ca6975798"

# GitHub
dotnet run --project src/Aviationexam.DependencyUpdater/Aviationexam.DependencyUpdater.csproj -- \
  --directory "/path/to/repo" --git-password "<token>" \
  GitHub --owner "<owner>" --repository "<repo>" --token "<token>" \
  --authentication-proxy-address "https://your-proxy.workers.dev"  # Optional
```

## Code Style Guidelines

### Import Organization
- Use file-scoped namespaces: `namespace X;`
- Group using statements WITHOUT blank lines between groups
- No specific ordering enforced (internal/Microsoft/System can be mixed)
- `.editorconfig` sets `dotnet_sort_system_directives_first = false`

### Naming Conventions
- **Classes/Records**: PascalCase (e.g., `DependencyAnalyzer`, `NugetPackageReference`)
- **Interfaces**: `I` prefix + PascalCase (e.g., `IRepositoryClient`)
- **Methods**: PascalCase with `Async` suffix for async methods (e.g., `AnalyzeDependenciesAsync`)
- **Private fields**: `_camelCase` prefix (e.g., `_labelCache`, `_ignoredDependenciesResolver`)
- **Properties**: PascalCase (e.g., `Platform`, `MinVersion`)
- **Parameters**: camelCase (e.g., `cancellationToken`, `ignoreResolver`)
- **Constants**: PascalCase (e.g., `MainLabelColor`)

### Code Formatting
- Indent: 4 spaces for C#, 2 spaces for csproj/props/config files
- Max line length: 200 characters
- End of line: CRLF
- Space after cast: `(string) obj` not `(string)obj`
- Insert final newline in all files

### Modern C# Features
- Use primary constructors: `public sealed class Foo(IBar bar, IBaz baz)`
- Use file-scoped namespaces: `namespace X;`
- Use top-level statements in Program.cs
- Use `required` for required properties
- Use record types for immutable data: `public sealed record Package(string Name, PackageVersion Version)`
- Target frameworks: .NET 9.0 and .NET 10.0

### Error Handling
- Use try/catch with structured logging via `ILogger`
- Guard logging with `logger.IsEnabled(LogLevel.X)` for performance
- Catch specific exceptions when possible (e.g., `ApiException`, `LibGit2SharpException`)
- Throw `NotImplementedException` for unsupported operations
- Log errors with context: `logger.LogError(exception, "Message with {Context}", contextValue)`

Example:
```csharp
try
{
    await SomeOperationAsync(cancellationToken);
}
catch (ApiException ex)
{
    if (logger.IsEnabled(LogLevel.Error))
    {
        logger.LogError(ex, "Failed to perform operation for {PackageName}", packageName);
    }
    throw;
}
```

### Async/Await and CancellationToken
- **CRITICAL**: All async methods MUST accept `CancellationToken` as the LAST parameter
- All async methods MUST end with `Async` suffix
- Always propagate `CancellationToken` to downstream async calls
- Use `ParallelOptions` with `CancellationToken` for parallel operations
- In tests, use `TestContext.Current.CancellationToken` - **NEVER** use `CancellationToken.None`

Example:
```csharp
public async Task<Result> ProcessDataAsync(
    string input,
    CancellationToken cancellationToken // Always last parameter
)
{
    await _client.FetchAsync(input, cancellationToken);
    
    await Parallel.ForEachAsync(items, new ParallelOptions 
    { 
        CancellationToken = cancellationToken 
    }, async (item, ct) =>
    {
        await ProcessItemAsync(item, ct);
    });
}
```

### Dependency Injection
- Register services in `ServiceCollectionExtensions` per project
- Use fluent chaining: `services.AddScoped<Foo>().AddScoped<Bar>()`
- Lifecycle guidelines:
  - `AddScoped` for request/operation-scoped services
  - `AddSingleton` for stateless factories and shared state
  - `AddKeyedScoped` for platform-specific implementations
- Use `TryAddScoped` for default implementations that can be overridden
- Use primary constructors for dependency injection
- **CRITICAL**: Always use generic `ILogger<T>` in constructors - NEVER use non-generic `ILogger` (breaks IoC container resolution)

Example:
```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNuget(
        this IServiceCollection services
    ) => services
        .AddScoped<DependencyAnalyzer>()
        .AddScoped<NugetUpdater>()
        .AddKeyedScoped<IRepositoryClient, RepositoryGitHubClient>(EPlatformSelection.GitHub);
}

// ✅ Correct - use generic ILogger<T>
public sealed class MyService(ILogger<MyService> logger)
{
    // ...
}

// ❌ Wrong - non-generic ILogger breaks IoC
public sealed class MyService(ILogger logger)
{
    // ...
}
```

## Testing Guidelines

### Test Framework
- Uses xUnit v3 with Microsoft.Testing.Platform runner
- Configured in `global.json` and each test project's `.csproj`
- Test files typically have `xunit.runner.json` (minimal config)

### Test Naming
- Test classes: Suffix with `Tests` (e.g., `DependencyAnalyzerTests`)
- Test methods: Descriptive names with pattern `MethodName_Scenario_ExpectedResult` or `MethodNameWorks`/`MethodNameFails`
- Examples: `ProcessDependencySet_WithNullMinVersion_SkipsPackage`, `ParseWorks`, `GetAllDependabotFilesWorks`

### Test Organization
- Follow Arrange-Act-Assert (AAA) pattern with explicit comments
- Use helper methods for reusable setup (e.g., `CreateDependencyAnalyzer()`)
- Use `using var` with `TemporaryDirectoryProvider` for file system tests

Example:
```csharp
[Fact]
public void ProcessDependencySet_WithNullMinVersion_SkipsPackage()
{
    // Arrange
    var ignoreResolver = new IgnoreResolver([], logger);
    var packageDependency = new PackageDependencyInfo("TestPackage", MinVersion: null);

    // Act
    var result = _processor.ProcessDependencySet(...);

    // Assert
    Assert.Empty(result);
}
```

### Mocking
- Use NSubstitute: `Substitute.For<IInterface>()`
- Argument matching: `Arg.Any<T>()`, `Arg.Is<T>(predicate)`
- Return values: `.Returns(value)` or `.Returns(Task.FromResult(value))`
- Exceptions: `.Throws<ExceptionType>()` (requires `NSubstitute.ExceptionExtensions`)

### Test Data
- Use `[Theory]` with `[ClassData(typeof(MyClassData))]` for complex data sets
- Implement `TheoryData<T1, T2, ...>` for reusable test data
- Embed JSON assets in test projects and load via `GetManifestResourceStream`

### CancellationToken in Tests
**CRITICAL**: Always use `TestContext.Current.CancellationToken` in test methods - **NEVER** use `CancellationToken.None`.

```csharp
[Fact]
public async Task ProcessAsync_Works()
{
    // Arrange
    var sut = CreateSystemUnderTest();

    // Act
    await sut.ProcessAsync(TestContext.Current.CancellationToken); // ✅ Correct

    // Assert
    Assert.NotNull(result);
}
```

### Test Diagnostics
- Use `TestContext.Current.AddAttachment(name, content)` to attach debug info to test results

### Integration vs Unit Tests
- **Integration tests**: Use real services with mocked external dependencies (e.g., `INugetVersionFetcher`)
- **Unit tests**: Pure mocks and deterministic inputs (dominant pattern)
- Integration tests document their scope with comments at the top of the class

## Project Structure

### Solution Organization
- Solution file: `Aviationexam.DependencyUpdater.slnx` (XML-based format)
- Central Package Management: `Directory.Packages.props` with `ManagePackageVersionsCentrally=true`
- Projects follow pattern: `Aviationexam.DependencyUpdater.<Component>`

### Key Projects
- **Aviationexam.DependencyUpdater** - CLI entry point with System.CommandLine
- **Aviationexam.DependencyUpdater.Nuget** - NuGet package resolution
- **Aviationexam.DependencyUpdater.Vcs.Git** - Git operations (LibGit2Sharp)
- **Aviationexam.DependencyUpdater.Repository.GitHub** - GitHub API integration
- **Aviationexam.DependencyUpdater.Repository.DevOps** - Azure DevOps integration
- **Aviationexam.DependencyUpdater.Interfaces** - Core abstractions
- **Aviationexam.DependencyUpdater.Common** - Shared domain models

### Platform Support
- Platforms selected via subcommands: `AzureDevOps` or `GitHub`
- Each platform has a Command class (e.g., `AzureDevOpsCommand`, `GitHubCommand`)
- Platform-specific implementations registered as keyed services
- Key enum: `EPlatformSelection` with `AzureDevOps`, `GitHub` members

## Common Patterns

### Configuration Binding
- CLI arguments bound to configuration objects using custom binders
- Common config: `.BindCommonConfiguration()` extension method
- Platform-specific config: registered in each Command's `ConfigureServices` method

### Ignore and Group Resolution
- `IgnoreResolver` - Determines which updates to skip (supports wildcards)
- `GroupResolver` - Groups related package updates (Dependabot groups)

### VCS Abstraction
- Git operations abstracted through `ISourceVersioningFactory` and `ISourceVersioningWorkspace`
- Implementation: LibGit2Sharp in Vcs.Git project

## Important Notes

- **Warnings as errors** enabled in Release configuration
- **NU1507 warning** suppressed globally (central package management)
- Uses Meziantou.Analyzer for code quality
- Blocks `Devlooped.SponsorLink` package (build fails if detected)
