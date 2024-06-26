﻿namespace MinimalEndpoints.SourceGenerator;

internal record EndpointGroup
{
    public EndpointNamespace Namespace { get; set; }

    public bool HasConfiguration { get; set; }

    public string Name { get; set; }

    public string Route { get; set; }
}
