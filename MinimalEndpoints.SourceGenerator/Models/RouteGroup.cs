namespace MinimalEndpoints.SourceGenerator.Models;

internal class RouteGroup
{
    public string Namespace { get; set; } = string.Empty;

    public string Class { get; set; } = string.Empty;

    public string Prefix { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public bool HasOwnConfiguration { get; set; }
}
