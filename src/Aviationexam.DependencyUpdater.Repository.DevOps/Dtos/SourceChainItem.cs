namespace Aviationexam.DependencyUpdater.Repository.DevOps.Dtos;

internal sealed class SourceChainItem
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Location { get; set; }
    public string? DisplayLocation { get; set; }
    public required int SourceType { get; set; }
}
