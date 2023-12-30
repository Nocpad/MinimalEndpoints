namespace MinimalEndpoints.SourceGenerator.Models;

internal sealed class Route
{
    public RouteGroup? Group { get; set; }

    public string Method { get; set; } = "";

    public string Template { get; set; } = "";

    public string[]? Policies { get; set; }

    public bool? RequireAuthorization { get; set; }
}
