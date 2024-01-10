using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace MinimalEndpoints.SourceGenerator;


[Generator]
public sealed class IncrementalSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
#if DEBUG
        //if (!Debugger.IsAttached)
        //{
        //    Debugger.Launch();
        //}
#endif

        Debug.WriteLine("Initialize code generator");

        RegisterPostInitialization(context);

        //IncrementalValuesProvider<ClassDeclarationSyntax> enumDeclarations = context.SyntaxProvider
        //    .CreateSyntaxProvider(
        //        predicate: static (s, _) => IsSyntaxTargetForGeneration(s), // select enums with attributes
        //        transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx)) // sect the enum with the [EnumExtensions] attribute
        //    .Where(static m => m is not null)!; // filter out attributed enums that we don't care about

        //// Combine the selected enums with the `Compilation`
        //IncrementalValueProvider<(Compilation, ImmutableArray<EnumDeclarationSyntax>)> compilationAndEnums
        //    = context.CompilationProvider.Combine(enumDeclarations.Collect());
        //context.RegisterPostInitializationOutput
    }

    //static bool IsSyntaxTargetForGeneration(SyntaxNode node)
    //{

    //    if (node is not ClassDeclarationSyntax classSyntax)
    //    {
    //        return false;
    //    }
    //    else
    //    {
    //        var root = classSyntax.SyntaxTree.GetRoot();
    //        root.get
    //    }

    //}

    // Generate the source using the compilation and enums
    //context.RegisterSourceOutput(compilationAndEnums,
    //        static (spc, source) => Execute(source.Item1, source.Item2, spc));


    private static void RegisterPostInitialization(IncrementalGeneratorInitializationContext context)
    {
        //context.AnalyzerConfigOptionsProvider.Select(e => e.GlobalOptions).

        context.RegisterPostInitializationOutput(async ctx =>
        {
            var httpAttriteTempate = await GetSourceFileTemplate("HttpMethodAttribute.template");
            string[] verbs = ["Get", "Post", "Put", "Delete"];

            foreach (var verb in verbs)
            {
                ctx.AddSource($"{verb}Attribute.g.cs", SourceText.From(httpAttriteTempate.Replace("{{METHOD}}", verb), Encoding.UTF8));
            }

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

        var contrent = await reader.ReadToEndAsync();

        // TODO: allow overriding the namespace of each source file
        return contrent.Replace("{{GeneratorNamespace}}", "Nocpad.AspNetCore.MinimalEndpoints");
    }
}
