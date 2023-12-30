using System.Collections.Generic;
using System.Linq;

namespace MinimalEndpoints.SourceGenerator.Models;

internal sealed class EndpointDefinitions
{
    public string RootNamespace { get; set; } = string.Empty;

    public bool HasValidationSupport { get; set; }

    public List<Endpoint> Endpoints { get; set; } = new();

    public List<EndpointsGroup> Groups { get; } = new();


    public void AddEndpoint(Endpoint endpoint)
    {
        if (endpoint.Group is not null && !string.IsNullOrEmpty(endpoint.Group.Prefix))
        {
            AddGroup(endpoint);
        }
        else
        {
            Endpoints.Add(endpoint);
        }
    }


    private void AddGroup(Endpoint endpoint)
    {
        if (Groups.FirstOrDefault(e => e.Prefix == endpoint.Group!.Prefix) is { } group)
        {
            group.Endpoints.Add(endpoint);
        }
        else
        {
            Groups.Add(new()
            {
                Prefix = endpoint.Group!.Prefix,
                Name = endpoint.Group.Name,
                Class = endpoint.Group.Class,
                Namespace = endpoint.Group.Namespace,
                HasOwnConfiguration = endpoint.Group.HasOwnConfiguration,
                Endpoints = { endpoint }
            });
        }
    }
}