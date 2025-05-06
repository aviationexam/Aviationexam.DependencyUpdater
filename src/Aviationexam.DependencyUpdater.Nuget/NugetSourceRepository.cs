using NuGet.Protocol.Core.Types;

namespace Aviationexam.DependencyUpdater.Nuget;

public record NugetSourceRepository(
    SourceRepository SourceRepository,
    SourceRepository? FallbackSourceRepository
);
