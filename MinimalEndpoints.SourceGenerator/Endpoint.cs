using System.Text;

namespace MinimalEndpoints.SourceGenerator;

internal record Endpoint
{
    public EndpointConfig Config { get; set; } = null!;

    public string Template { get; set; } = string.Empty;

    public string EndpointMethod { get; set; } = string.Empty;

    public string HttpMethod { get; set; } = string.Empty;

    public EndpointNamespace Namespace { get; set; } = null!;

    public bool HasConfiguration { get; set; }

    public EndpointGroup? Group { get; set; }

    public void AppenEndpointString(StringBuilder sb, string endpointParent, string variableName)
    {
        sb.Append($$"""
                var {{variableName}} = {{endpointParent}}.MapMethods("{{Template}}", new[] {"{{HttpMethod}}"}, {{Namespace}}.{{EndpointMethod}}){{Config}};

        """);


        if (HasConfiguration)
        {
            sb.Append($$"""
                        {{Namespace}}.Configure({{variableName}});

                """);
        }
    }
}
