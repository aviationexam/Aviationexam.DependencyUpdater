namespace Aviationexam.DependencyUpdater.Repository.DevOps.Dtos;

public sealed class SourcePage
{
    public required string Url { get; set; }
    public required string RouteId { get; set; }
    public required RouteValues RouteValues { get; set; }
}
