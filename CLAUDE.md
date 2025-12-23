# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Aviationexam.DependencyUpdater is a .NET command-line tool for automating dependency updates in projects using Git repositories and NuGet feeds. It supports multiple repository platforms (Azure DevOps and GitHub) and reads Dependabot-style configuration files to process updates accordingly.

## Build Commands

### Build the solution
```bash
dotnet restore --nologo
dotnet build --no-restore --nologo --configuration Release
```

### Run tests
```bash
# Run all tests
dotnet test --no-build --configuration Release

# Run tests with TRX report output
dotnet test --no-build --configuration Release --results-directory TestResults --report-trx
```

### Code formatting
```bash
# Check formatting (CI mode)
dotnet format --no-restore --verify-no-changes -v diag

# Apply formatting
dotnet format
```

### Run the tool locally
The main project `Aviationexam.DependencyUpdater` is the CLI entry point. Run using platform-specific subcommands:

**Azure DevOps:**
```bash
dotnet run --project src/Aviationexam.DependencyUpdater/Aviationexam.DependencyUpdater.csproj -- \
  --directory "/path/to/repo" \
  --git-password "<token>" \
  AzureDevOps \
  --organization "<org>" \
  --project "<project-id>" \
  --repository "<repo-id>" \
  --pat "<pat>" \
  --account-id "<account-id>" \
  --nuget-project "<nuget-project-id>" \
  --nuget-feed-id "<feed-id>" \
  --nuget-service-host "<service-host>" \
  --access-token-resource-id "499b84ac-1321-427f-aa17-267ca6975798"
```

**GitHub:**
```bash
dotnet run --project src/Aviationexam.DependencyUpdater/Aviationexam.DependencyUpdater.csproj -- \
  --directory "/path/to/repo" \
  --git-password "<token>" \
  GitHub \
  --owner "<owner>" \
  --repository "<repo>" \
  --token "<token>"
```

**Required GitHub PAT Scopes:**
- `repo` - Full control of private repositories

## Solution Structure

The solution uses a modular architecture with clear separation of concerns:

- **Aviationexam.DependencyUpdater** - Main CLI application entry point with command-line argument parsing (System.CommandLine)
  - **Commands/** - Platform-specific command classes (AzureDevOpsCommand, GitHubCommand) that encapsulate CLI options and service configuration
- **Aviationexam.DependencyUpdater.ConfigurationParser** - Parses Dependabot v2 YAML configuration files and converts them to internal models
- **Aviationexam.DependencyUpdater.Common** - Shared domain models and utilities (Package, IgnoreResolver, GroupResolver, etc.)
- **Aviationexam.DependencyUpdater.Nuget** - NuGet-specific package resolution and version handling
- **Aviationexam.DependencyUpdater.Vcs.Git** - Git operations using LibGit2Sharp (platform-agnostic)
- **Aviationexam.DependencyUpdater.Repository.DevOps** - Azure DevOps implementation (pull requests, artifact feeds, upstream ingestion)
- **Aviationexam.DependencyUpdater.Repository.GitHub** - GitHub implementation (pull requests, labels, reviewers, milestones)
- **Aviationexam.DependencyUpdater.Interfaces** - Core interfaces and contracts including:
  - Repository abstractions (`IRepositoryClient`, `IPackageFeedClient`, `IRepositoryPlatformConfiguration`)
  - Package management interfaces
  - VCS abstractions
- **Aviationexam.DependencyUpdater.DefaultImplementations** - Default implementations for file system and environment variable access
- **Aviationexam.DependencyUpdater.Constants** - Shared constants
- **Aviationexam.DependencyUpdater.TestsInfrastructure** - Shared test utilities

## Project Configuration

- **Solution file**: `Aviationexam.DependencyUpdater.slnx` (XML-based solution format)
- **Central Package Management**: Uses `Directory.Packages.props` with `ManagePackageVersionsCentrally` enabled
- **Target frameworks**: Multi-targeting .NET 9.0 and .NET 10.0
- **Test framework**: xUnit v3 with Microsoft.Testing.Platform runner (configured in global.json)

## Key Architecture Patterns

### Dependency Injection
The application uses Microsoft.Extensions.DependencyInjection throughout. Service registration is organized via `ServiceCollectionExtensions` classes in each project.

### Configuration Binding
Command-line arguments are bound to configuration objects using custom binders (e.g., `SourceConfigurationBinder`, `AzureDevOpsConfigurationBinder`). The `ServiceCollectionExtensions` class uses C# 13's extension block syntax to provide fluent binder registration methods.

Platform selection is handled via subcommands (`AzureDevOps` or `GitHub`), with each platform having its own Command class that encapsulates platform-specific options and configuration. Common configuration is shared across platforms via the `.BindCommonConfiguration()` extension method. All CLI options explicitly define `Arity = ArgumentArity.ExactlyOne` for clear argument expectations.

### Command Pattern for Platform Selection
Each repository platform is represented by a dedicated Command class inheriting from `System.CommandLine.Command`:
- **Command Class Structure**: Each command (e.g., `AzureDevOpsCommand`) encapsulates all platform-specific CLI options as private fields
- **Constructor Initialization**: Options are initialized and added to the command's `Options` collection via `Options.Add()`
- **Service Configuration**: Each command implements a `ConfigureServices(IServiceCollection, ParseResult, ...)` method that:
  - Calls `.BindCommonConfiguration()` for shared setup
  - Registers platform-specific binders and configurations
  - Wires up keyed services for that platform
- **Program.cs Integration**: Commands are instantiated, configured with `SetAction()`, and added as subcommands to the root command
- **Benefits**: Clear separation of platform concerns, simplified Program.cs (~70 lines vs ~200 lines), easier to add new platforms

### VCS Abstraction
Git operations are abstracted through `ISourceVersioningFactory` and `ISourceVersioningWorkspace` interfaces, with implementations in the Vcs.Git project.

### Package Management
- `IPackageManager` interface abstracts package operations
- NuGet implementation supports reading package references from project files and resolving versions
- Caching layer to minimize redundant API calls

### Ignore and Group Resolution
- `IgnoreResolver` - Determines which package updates to skip based on Dependabot ignore rules (supports wildcards)
- `GroupResolver` - Groups related package updates together according to Dependabot group configuration

### Multi-Platform Repository Support
The tool supports multiple repository platforms through a clean abstraction layer:

**Platform Selection:**
- Platform is selected via subcommands: `AzureDevOps` or `GitHub`
- Each platform has its own Command class in the `Commands` namespace (e.g., `AzureDevOpsCommand`, `GitHubCommand`)
- Platform-specific configuration arguments are scoped to their respective subcommands (no prefix needed since subcommand provides context)
- Platform selection enum: `EPlatformSelection` with members `AzureDevOps`, `GitHub`
- Command classes inherit from `System.CommandLine.Command` and encapsulate their options and service configuration

**Core Abstractions** (in `Aviationexam.DependencyUpdater.Interfaces.Repository`):
- `IRepositoryClient` - Platform-agnostic interface for pull request operations
  - List, get, create, update, and abandon pull requests
  - Implemented by `RepositoryAzureDevOpsClient` and `RepositoryGitHubClient`
- `IPackageFeedClient` - Optional interface for platform-specific package feed operations
  - Azure Artifacts upstream ingestion for Azure DevOps (via `AzureArtifactsPackageFeedClient`)
  - Not used for GitHub (wrapped in `Optional<IPackageFeedClient>`)
- `IRepositoryPlatformConfiguration` - Base interface for platform-specific configurations
  - Contains `EPlatformSelection Platform { get; }` property
  - Implemented by `AzureDevOpsConfiguration` and `GitHubConfiguration`

**Architecture Layers:**
1. **VCS Layer** (platform-agnostic) - Git operations via LibGit2Sharp
2. **Repository Platform Layer** - PR and feed operations specific to Azure DevOps or GitHub
3. **Package Management Layer** (platform-agnostic) - NuGet package resolution

**DI Registration (Keyed Services Pattern):**
- Platform implementations registered as keyed services in each platform's `ServiceCollectionExtensions`
  ```csharp
  // Azure DevOps
  .AddKeyedScoped<IRepositoryClient, RepositoryAzureDevOpsClient>(EPlatformSelection.AzureDevOps)
  .AddKeyedScoped<IPackageFeedClient, AzureArtifactsPackageFeedClient>(EPlatformSelection.AzureDevOps)

  // GitHub
  .AddKeyedScoped<IRepositoryClient, RepositoryGitHubClient>(EPlatformSelection.GitHub)
  // No IPackageFeedClient for GitHub
  ```
- Main `ServiceCollectionExtensions.AddRepositoryPlatform()` resolves services using keyed lookup:
  ```csharp
  .AddScoped<IRepositoryClient>(sp =>
      sp.GetRequiredKeyedService<IRepositoryClient>(
          sp.GetRequiredService<IRepositoryPlatformConfiguration>().Platform
      ))
  ```
- Platform key comes from `IRepositoryPlatformConfiguration.Platform` property
- Conditional registration of `Optional<IPackageFeedClient>` - returns null if platform doesn't support feeds
- All platform-specific registrations consolidated in their respective projects

**Key Implementation Details:**
- **Keyed Services**: Uses .NET's built-in keyed service registration instead of manual factory pattern
- **Platform Resolution**: Platform is determined from configuration's `Platform` property at runtime
- **Extension Block Syntax**: ServiceCollectionExtensions uses C# 13 extension blocks for fluent API
- **Namespace Organization**: Repository abstractions in `Aviationexam.DependencyUpdater.Interfaces.Repository`
- **Single Responsibility**: Each platform project registers its own implementations with the appropriate key

## Configuration File Format

The tool reads standard Dependabot v2 configuration, typically at `.github/dependabot.yml`.

The Dependabot schema is automatically downloaded from https://www.schemastore.org/dependabot-2.0.json during build and used by Corvus.Json.SourceGenerator to generate strongly-typed models. The schema file is downloaded to `src/Aviationexam.DependencyUpdater.ConfigurationParser/dependabot-2.0.json` if it doesn't exist (see the `DownloadDependabotSchema` target in the ConfigurationParser.csproj).

Key configuration features:
- Package ecosystem specification (e.g., "nuget")
- Update schedules and grouping rules
- Ignore patterns for dependencies or version ranges
- Directory-based configurations

## Platform-Specific Integrations

### Azure DevOps

The tool has undocumented Azure DevOps API integrations in `Repository.DevOps`:
- `AzureDevOpsUndocumentedClient` - Handles upstream package ingestion for Azure Artifacts feeds
- Supports authentication via PAT or optional AZ CLI sidecar service
- Can fetch and ingest package versions from upstream sources (e.g., nuget.org) into private feeds

### GitHub

The tool uses Octokit.NET for GitHub API integration in `Repository.GitHub`:
- `RepositoryGitHubClient` - Handles pull request operations with GitHub API
- Automatic label creation and management for dependency PRs
- Support for reviewers and milestones
- Rate limiting handling with exponential backoff retry logic
- Authentication via GitHub Personal Access Token

**Known Limitations:**
- No auto-merge support (GitHub GraphQL API needed, not available in Octokit REST API)

## Development Notes

- **Warnings as errors**: Enabled in Release configuration
- **Blocked packages**: Build fails if `Devlooped.SponsorLink` is detected as a dependency
- **NU1507 warning**: Suppressed globally (related to central package management)
- **Analyzers**: Uses Meziantou.Analyzer for code quality
- **C# Language Version**: Uses C# 13 features including extension block syntax in ServiceCollectionExtensions
- **Target Frameworks**: Multi-targets .NET 9.0 and .NET 10.0

## Testing Guidelines

### CancellationToken Usage
**CRITICAL**: Never use `CancellationToken.None` in test code. Always use:
- `TestContext.Current.CancellationToken` in test methods
- `cancellationToken` parameter when passed to methods

This ensures tests can be properly cancelled and respect test timeouts.

### Integration Tests with Real NuGet Data
When writing integration tests for `DependencyAnalyzer`:
- **Mock only INugetVersionFetcher**: Mock `FetchPackageVersionsAsync()` to return filtered real metadata
- **Skip transitive dependencies**: Mock `FetchPackageMetadataAsync()` to return `null` for simplified tests
- **Use mock SourceRepository**: Don't use real repository instances in tests (use `Substitute.For<SourceRepository>()`)
- **No fallback mocks**: Be explicit about which packages are being tested - if mock isn't set up, it should fail

Example pattern:
```csharp
// Fetch real metadata from nuget.org
var metadata = await FetchRealPackageMetadataAsync(
    "PackageName",
    ["1.0.0", "1.0.1", "1.1.0"]  // Only versions needed for test
);

// Mock to return real data
mockVersionFetcher
    .FetchPackageVersionsAsync(
        Arg.Any<SourceRepository>(),
        Arg.Is<NugetDependency>(d => d.NugetPackage.GetPackageName() == "PackageName"),
        Arg.Any<SourceCacheContext>(),
        Arg.Any<CancellationToken>())
    .Returns(Task.FromResult(metadata));

// Skip transitive dependency validation
mockVersionFetcher
    .FetchPackageMetadataAsync(
        Arg.Any<SourceRepository>(),
        Arg.Any<Package>(),
        Arg.Any<SourceCacheContext>(),
        Arg.Any<CancellationToken>())
    .Returns(Task.FromResult<IPackageSearchMetadata?>(null));
```

### Common Development Workflows

### Running a specific test
```bash
dotnet test src/Aviationexam.DependencyUpdater.Nuget.Tests/Aviationexam.DependencyUpdater.Nuget.Tests.csproj --filter "FullyQualifiedName~TestName"
```

### Working with Git operations
Git operations are in `Vcs.Git` project using LibGit2Sharp. The `GitSourceVersioningFactory` creates workspace instances that manage branches, commits, and pull requests.

### Adding new package sources
Extend `IPackageManager` interface and create implementation similar to the Nuget project structure. Register in DI container via `ServiceCollectionExtensions`.

### Adding new repository platforms
To add support for a new platform (e.g., GitLab):
1. **Add to Enum**: Add new value to `EPlatformSelection` enum (e.g., `GitHub`)
2. **Create Platform Project**: Create `Aviationexam.DependencyUpdater.Repository.<Platform>` project
3. **Implement Configuration**: Create configuration class implementing `IRepositoryPlatformConfiguration`
   - Implement `Platform` property to return the correct enum value (e.g., `EPlatformSelection.GitHub`)
   - Use appropriate prefix for platform-specific configs (e.g., `Azure` for Azure DevOps, `GitHub` for GitHub)
4. **Implement Clients**:
   - Implement `IRepositoryClient` for PR operations
   - Optionally implement `IPackageFeedClient` if the platform has package feed features
5. **Register as Keyed Services**: In platform's `ServiceCollectionExtensions`:
   ```csharp
   .AddKeyedScoped<IRepositoryClient, RepositoryGitHubClient>(EPlatformSelection.GitHub)
   .AddKeyedScoped<IPackageFeedClient, GitHubPackageFeedClient>(EPlatformSelection.GitHub) // if applicable
   ```
6. **Create Command Class**: Create `Commands/<Platform>Command.cs`:
   - Inherit from `System.CommandLine.Command`
   - Base constructor call: `base(nameof(EPlatformSelection.<Platform>), "<description>")`
   - Declare platform-specific options as private fields (e.g., `--organization`, `--repository`)
   - Initialize and add options in constructor via `Options.Add()`
   - Implement `ConfigureServices(IServiceCollection, ParseResult)` method to set up DI bindings
7. **Create Configuration Binder**: Create binder to map CLI arguments to configuration object
8. **Wire Up in Program.cs**:
   - Instantiate the command class
   - Call `SetAction(DefaultCommandHandler.GetHandler(...))` with service configuration
   - Add subcommand to root command via `rootCommand.Subcommands.Add()`

**Note**: Argument names don't need platform prefixes (e.g., use `--organization` not `--azure-organization`) since the subcommand already provides the platform context.

The keyed services pattern automatically resolves the correct implementation at runtime based on the configuration's `Platform` property. The command pattern keeps platform-specific CLI options organized in their own classes.

## Recent Refactorings

### DependencyUpdateProcessor Extraction (2024)

Refactored `ProcessDependenciesToUpdate` method from `DependencyAnalyzer` to improve testability by extracting the logic into a new, dedicated service class.

#### New Service Class: `DependencyUpdateProcessor`
**Location:** `src/Aviationexam.DependencyUpdater.Nuget/Services/DependencyUpdateProcessor.cs`

Extracted the dependency processing logic into a standalone, testable service with the following methods:

- **`ProcessDependenciesToUpdate`**: Main entry point that processes all dependencies and returns results
- **`ProcessDependencySet`**: Processes a single dependency set (now public and directly testable)
- **`ProcessTargetFrameworks`**: Determines flags for each target framework
- **`DetermineFrameworkFlag`**: Determines the specific flag for a target framework
- **`IsAtCorrectVersion`**: Helper to check if a package is at the correct version

#### New Result Type: `DependencyProcessingResult`
**Location:** `src/Aviationexam.DependencyUpdater.Nuget/Models/DependencyProcessingResult.cs`

A record that encapsulates the output of dependency processing:
```csharp
public sealed record DependencyProcessingResult(
    IDictionary<Package, IDictionary<NugetTargetFramework, EDependencyFlag>> PackageFlags,
    Queue<(Package Package, IReadOnlyCollection<NugetTargetFramework> NugetTargetFrameworks)> DependenciesToCheck
);
```

#### Updated `DependencyAnalyzer`
- Injected `DependencyUpdateProcessor` via constructor
- Removed `ignoredDependenciesResolver` parameter (now used by `DependencyUpdateProcessor`)
- Removed private `ProcessDependenciesToUpdate` and `ProcessDependencySet` methods
- Updated `ProcessPackageMetadata` to use `dependencyUpdateProcessor.ProcessDependencySet`

Before:
```csharp
var packageFlags = new Dictionary<Package, IDictionary<NugetTargetFramework, EDependencyFlag>>();
var dependenciesToCheck = new Queue<(Package, IReadOnlyCollection<NugetTargetFramework>)>();

ProcessDependenciesToUpdate(
    ignoreResolver,
    currentPackageVersionsPerTargetFramework,
    dependencyToUpdate,
    packageFlags,
    dependenciesToCheck
);
```

After:
```csharp
var processingResult = dependencyUpdateProcessor.ProcessDependenciesToUpdate(
    ignoreResolver,
    currentPackageVersionsPerTargetFramework,
    dependencyToUpdate
);

var packageFlags = processingResult.PackageFlags;
var dependenciesToCheck = processingResult.DependenciesToCheck;
```

#### Test Suite: `DependencyUpdateProcessorTests`
**Location:** `src/Aviationexam.DependencyUpdater.Nuget.Tests/DependencyUpdateProcessorTests.cs`

Comprehensive unit tests demonstrating the improved testability:

**Unit Tests (Fact):**
- ✅ `ProcessDependencySet_WithNullMinVersion_SkipsPackage`
- ✅ `ProcessDependencySet_WithSinglePackage_PopulatesPackageFlags`
- ✅ `ProcessDependencySet_WithAlreadyInstalledVersion_MarksAsValid`
- ✅ `ProcessDependencySet_WithIgnoredDependency_MarksAsContainsIgnoredDependency`
- ✅ `ProcessDependencySet_WithMultiplePackages_ReturnsAllPackages`
- ✅ `ProcessDependencySet_CalledTwiceWithSamePackage_DoesNotReprocessTargetFramework`

**Theory Tests (using FutureDependenciesClassData):**
- ✅ `ProcessDependenciesToUpdate_WithRealWorldData_ProcessesAllDependencies`
- ✅ `ProcessDependenciesToUpdate_WithRealWorldData_QueuesUnknownDependencies`
- ✅ `ProcessDependenciesToUpdate_WithRealWorldData_CountsExpectedPackages`

The theory tests use **ALL** dependencies from `FutureDependenciesClassData` (real-world Aviationexam.Core.Common packages), validating the complete `ProcessDependenciesToUpdate` workflow with multiple packages, target frameworks, and complex transitive dependencies.

#### Benefits
1. **Improved Testability** - Public methods with clear inputs and outputs
2. **Single Responsibility** - Separation between orchestration and processing logic
3. **Better Encapsulation** - Dependencies encapsulated within appropriate services
4. **Clearer API** - Explicit return types instead of side effects
5. **Easier to Extend** - New processing strategies can be added without modifying orchestration code
