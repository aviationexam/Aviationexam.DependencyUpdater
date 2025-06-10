using NuGet.Protocol.Core.Types;

namespace Aviationexam.DependencyUpdater.Nuget.Models;

public record NugetSourceRepository(
    SourceRepository SourceRepository,
    SourceRepository? FallbackSourceRepository
);
