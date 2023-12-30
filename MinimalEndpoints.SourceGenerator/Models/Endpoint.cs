using System.Collections.Generic;

namespace MinimalEndpoints.SourceGenerator.Models;
internal sealed class Endpoint
{
    public Namespace Namespace { get; set; }

    public string Class { get; set; } = "";

    public string MethodName { get; set; } = "";

    public bool HasOwnConfiguration { get; set; }

    public RouteGroup? Group { get; set; }

    public string HttpMethod { get; set; } = "";

    public string Template { get; set; } = "";

    public string[]? Policies { get; set; }

    public bool? RequireAuthorization { get; set; }

    public List<Validator>? Validators { get; set; }
}
