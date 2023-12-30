namespace MinimalEndpoints.SourceGenerator;

internal sealed class DiagnosticCode(string code, string title, string description)
{
    public string Code { get; } = code;

    public string Title { get; } = title;

    public string Description { get; } = description;
}
