using Microsoft.CodeAnalysis;

namespace MinimalEndpoints.SourceGenerator;

internal record EndpointNamespace
{
    public EndpointNamespace(INamespaceSymbol @namespace, string groupName)
    {
        Value = $"{(@namespace.IsGlobalNamespace ? "global::" : @namespace.ToDisplayString() + ".")}{groupName}";
    }

    public string Value { get; private set; }

    public override string ToString() => Value;
}