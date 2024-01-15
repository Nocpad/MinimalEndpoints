using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using MinimalEndpoints.SourceGenerator.Models;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace MinimalEndpoints.SourceGenerator;


[Generator]
public sealed class IncrementalSourceGenerator : IIncrementalGenerator
{
    private static string _namespace = "";
    private const string EndpointAttribute = "Nocpad.AspNetCore.MinimalEndpoints.EndpointAttribute";


    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
#if DEBUG
        if (!Debugger.IsAttached)
        {
            Debugger.Launch();
        }
#endif

        Debug.WriteLine("Initialize code generator");

        RegisterPostInitialization(context);

        IncrementalValuesProvider<Endpoint> endpoints = context.SyntaxProvider.ForAttributeWithMetadataName(
            EndpointAttribute,
            predicate: (_, _) => true,
            transform: GetSemanticTargetForGeneration
        ).Where(static m => m is not null)!;

        //context.RegisterSourceOutput(classDeclarations, static (spc, source) => ...(source, spc));
    }

    static bool IsSyntaxTargetForGeneration(SyntaxNode node) => node is ClassDeclarationSyntax c && c.AttributeLists.Count > 0;


    static Endpoint? GetSemanticTargetForGeneration(GeneratorAttributeSyntaxContext context, CancellationToken ct)
    {
        if (context.TargetSymbol is not INamedTypeSymbol classSymbol)
        {
            // nothing to do if this type isn't available
            return null;
        }

        ct.ThrowIfCancellationRequested();

        var classMembers = classSymbol.GetMembers();

        foreach (var member in classMembers)
        {
            if (member is not IMethodSymbol)
            {
                continue;
            }

        }


        return null;
    }



    private static void RegisterPostInitialization(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(async ctx =>
        {
            var httpAttriteTempate = await GetSourceFileTemplate("HttpMethodAttribute.template");
            string[] verbs = ["Get", "Post", "Put", "Delete"];

            foreach (var verb in verbs)
            {
                ctx.AddSource($"{verb}Attribute.g.cs", SourceText.From(httpAttriteTempate.Replace("{{METHOD}}", verb), Encoding.UTF8));
            }

            ctx.AddSource("EndpointInterfaces.g.cs", SourceText.From(await GetSourceFileTemplate("Interfaces.template"), Encoding.UTF8));
            ctx.AddSource("EndpointAttribute.g.cs", SourceText.From(await GetSourceFileTemplate("EndpointAttribute.template"), Encoding.UTF8));
        });
    }


    private static async Task<string> GetSourceFileTemplate(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        string resourcePath = assembly
            .GetManifestResourceNames()
            .FirstOrDefault(e => e.Contains(resourceName));

        using Stream stream = assembly.GetManifestResourceStream(resourcePath);
        using StreamReader reader = new(stream);

        var content = await reader.ReadToEndAsync();

        // TODO: allow overriding the namespace of each source file
        return content.Replace("{{GeneratorNamespace}}", "Nocpad.AspNetCore.MinimalEndpoints");
    }
}
