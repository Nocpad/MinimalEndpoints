
using Microsoft.CodeAnalysis;

namespace MinimalEndpoints.SourceGenerator;

internal sealed record EndpointConfig
{
    public bool Active { get; set; }

    public ITypeSymbol? Validator { get; set; }

    public bool? RequireAuthorization { get; set; }

    public string[]? Policies { get; set; }
}
