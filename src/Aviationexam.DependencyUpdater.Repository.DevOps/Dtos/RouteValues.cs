namespace Aviationexam.DependencyUpdater.Repository.DevOps.Dtos;

public sealed class RouteValues
{
    public required string Project { get; set; }
    public required string Wildcard { get; set; }
    public required string Controller { get; set; }
    public required string Action { get; set; }
    public required string ServiceHost { get; set; }
}
