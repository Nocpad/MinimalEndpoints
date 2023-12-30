namespace MinimalEndpoints.SourceGenerator.Models;

internal sealed class Namespace
{
    public string Name { get; set; }

    public bool IsGlobalNamespace { get; set; }

    public override string ToString() => IsGlobalNamespace ? Name : $"{Name}.";
}