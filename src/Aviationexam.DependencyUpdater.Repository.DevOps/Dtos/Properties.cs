namespace Aviationexam.DependencyUpdater.Repository.DevOps.Dtos;

internal sealed class Properties
{
    public required string ProjectId { get; set; }
    public required string FeedId { get; set; }
    public required string Protocol { get; set; }
    public required string PackageName { get; set; }
    public required SourcePage SourcePage { get; set; }
}
