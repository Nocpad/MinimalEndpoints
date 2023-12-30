using Microsoft.CodeAnalysis;

namespace MinimalEndpoints.SourceGenerator;
internal static class GeneratorExecutionContextExtensions
{
    internal static void AddDiagnostic(this GeneratorExecutionContext context, DiagnosticCode code, DiagnosticSeverity severity, Location? location = null, params object[] args)
    {
        context.ReportDiagnostic(Diagnostic.Create(new(code.Code, code.Title, args is { Length: > 0 } ? string.Format(code.Description, args) : code.Description, "Generator", severity, true), location ?? Location.None));
    }
}
