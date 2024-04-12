using System.Text;

namespace MinimalEndpoints.SourceGenerator;

internal sealed record Endpoint
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
        sb.Append("        ")
            .Append($"var {variableName} = {endpointParent}")
            .Append($".MapMethods(\"{Template}\", new[] {{\"{HttpMethod}\"}}, {Namespace}.{EndpointMethod})");

        if (Config.Validator is { })
        {
            sb.AppendLine()
                .Append("        ")
                .Append("        ")
                .Append($".AddEndpointFilter<AbstractValidationFilter<{Config.Validator}>>()");
        }

        if (Config.RequireAuthorization is not null)
        {
            sb.AppendLine()
                .Append("        ")
                .Append("        ")
                .Append(Config.RequireAuthorization == true ? ".RequireAuthorization()" : ".AllowAnonymous()");
        }

        if (Config.Policies is { Length: > 0 })
        {
            sb.AppendLine()
                .Append("        ")
                .Append("        ")
                .Append($$""".RequireAuthorization("{{string.Join("\", \"", Config.Policies)}}")""");
        }

        sb.Append(";");

        if (HasConfiguration)
        {
            sb.AppendLine()
                .Append("        ")
                .Append($"{Namespace}.Configure({variableName});");
        }

        sb.AppendLine();
    }
}
