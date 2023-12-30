namespace MinimalEndpoints.SourceGenerator;

internal static class DiagnosticCodes
{
    public static DiagnosticCode SymbolNotFound = new("PCE001", "Symbol not found", "Cannot find symbol with name '{0}'. Make sure you are referencing MinimalEndpoints");

    public static DiagnosticCode ClassWithNoEndpointMethod = new("PCE002", "Class does not include any endpoint method.", "No method found having an 'HttpBase' attribute.");

    public static DiagnosticCode ClassWithMultipleEndpoints = new("PCE003", "Multiple endpoint declarations detected.", "For security reasons multiple endpoint declarations are not allowed when a class implements the IEndpointConfiguration since the Configure method will be common on every endpoint method. If you think it's safe you can add the AllowMultipleEndpoints attribute on the class to disable this message.");

    public static DiagnosticCode NonStaticMethod = new("PCE004", "Non static endpoints methods are not supported.", "Non static endpoints methods are not supported.");
}