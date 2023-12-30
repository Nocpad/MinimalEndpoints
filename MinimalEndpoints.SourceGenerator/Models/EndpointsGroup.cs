using System.Collections.Generic;

namespace MinimalEndpoints.SourceGenerator.Models;

internal sealed class EndpointsGroup : RouteGroup
{
    public List<Endpoint> Endpoints { get; set; } = new();
}
