using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace MinimalEndpoints.SourceGenerator;


[Generator]
public sealed class IncrementalSourceGenerator : IIncrementalGenerator
{
    private const string EndpointAttribute = "Nocpad.AspNetCore.MinimalEndpoints.EndpointAttribute";
    private const string EndpointWithGroupAttribute = "Nocpad.AspNetCore.MinimalEndpoints.EndpointAttribute`1";
    private const string EndpointWithConfiguration = "Nocpad.AspNetCore.MinimalEndpoints.IEndpointConfiguration";
    private const string EndpointGroupWithConfiguration = "Nocpad.AspNetCore.MinimalEndpoints.IEndpointGroupConfiguration";
    private const string ValidateAttribute = "Nocpad.AspNetCore.MinimalEndpoints.ValidateAttribute`1";


    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
#if DEBUG
        Debugger.Launch();
#endif
        context.RegisterPostInitializationOutput(AddStaticSources);


        IncrementalValueProvider<GeneratorOptions> fluentValidationEndpointFilter = context.CompilationProvider.Select(static (compilation, cancellationToken) =>
        {
            return new GeneratorOptions
            {
                HasFluentValidationSupport = compilation.ReferencedAssemblyNames.Any(e => e.Name.Equals("FluentValidation", StringComparison.OrdinalIgnoreCase)),
            };
        });

        IncrementalValuesProvider<List<Endpoint>> endpoints = context.SyntaxProvider.ForAttributeWithMetadataName(
            EndpointAttribute,
            predicate: (_, _) => true,
            transform: GetSemanticTargetForGeneration
        ).Where(static m => m is { Count: > 0 });

        IncrementalValuesProvider<List<Endpoint>> endpointsWithGroup = context.SyntaxProvider.ForAttributeWithMetadataName(
            EndpointWithGroupAttribute,
            predicate: (_, _) => true,
            transform: GetSemanticTargetForGeneration
        ).Where(static m => m is { Count: > 0 });

        var compilation = context.CompilationProvider;

        context.RegisterImplementationSourceOutput(compilation.Combine(endpoints.Collect()), static (spc, source) => Execute(source, spc, "MapIndividualEndpoints", "Individual"));
        context.RegisterImplementationSourceOutput(compilation.Combine(endpointsWithGroup.Collect()), static (spc, source) => Execute(source, spc, "MapEndpointGroups", "Groups"));

        context.RegisterSourceOutput(fluentValidationEndpointFilter, static (scp, source) =>
        {
            if (!source.HasFluentValidationSupport)
            {
                return;
            }

            scp.AddSource("AbstractValidationFilter.g.cs", """
                using FluentValidation;
                using Microsoft.AspNetCore.Http;
                using Microsoft.Extensions.DependencyInjection;
                using System.Linq;
                using System;
                using System.Threading.Tasks;

                namespace Nocpad.AspNetCore.MinimalEndpoints;

                [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
                public sealed class ValidateAttribute<T> : Attribute { }

                [System.Diagnostics.StackTraceHidden]
                public sealed class AbstractValidationFilter<T> : IEndpointFilter
                {
                    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
                    {
                        var validator = context.HttpContext.RequestServices.GetService<IValidator<T>>();

                        if (validator is not null)
                        {
                            var entity = context.Arguments.OfType<T>().FirstOrDefault();

                            return entity switch
                            {
                                null => Results.Problem("Could not find type to validate."),
                                _ => await ValidateEntity(context, next, validator, entity)
                            };
                        }

                        return await next(context);
                    }


                    private static async ValueTask<object?> ValidateEntity(EndpointFilterInvocationContext context, EndpointFilterDelegate next, IValidator<T>? validator, T? entity)
                    {
                        var validation = await validator.ValidateAsync(entity);
                        if (validation.IsValid)
                        {
                            return await next(context);
                        }

                        return Results.ValidationProblem(validation.ToDictionary());
                    }
                }
                """);
        });
    }

    internal static void AddStaticSources(IncrementalGeneratorPostInitializationContext context)
    {
        StringBuilder sb = new(1024);

        sb.Append($$"""
            // <auto-generated />

            using Microsoft.AspNetCore.Builder;
            using Microsoft.AspNetCore.Http;
            using Microsoft.AspNetCore.Routing;
            using Microsoft.Extensions.DependencyInjection;
            using System;
            using System.Linq;
            using System.Threading.Tasks;

            namespace Nocpad.AspNetCore.MinimalEndpoints;


            [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
            public sealed class EndpointAttribute : Attribute
            {
                public bool Active { get; set; }
            }

            [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
            public sealed class EndpointAttribute<TGroup> : Attribute where TGroup : IEndpointGroup
            {
                public bool Active { get; set; }
            }

            public interface IEndpointConfiguration
            {
                abstract static void Configure(RouteHandlerBuilder builder);
            }

            public interface IEndpointGroup
            {
                abstract static string Name { get; }

                abstract static string Route { get; }
            }

            public interface IEndpointGroupConfiguration : IEndpointGroup
            {
                abstract static void Configure(RouteGroupBuilder group);
            }


            public static partial class MapMinimalEndpointsExtensions
            {
                public static WebApplication MapMinimalEndpoints(this WebApplication app)
                {
                    return app
                        .MapIndividualEndpoints()
                        .MapEndpointGroups();
                }

                public static partial WebApplication MapIndividualEndpoints(this WebApplication app);

                public static partial WebApplication MapEndpointGroups(this WebApplication app);
            }
            """);

        HashSet<string> verbs = new(["Get", "Post", "Put", "Delete"], StringComparer.OrdinalIgnoreCase);

        foreach (var verb in verbs)
        {
            sb.AppendLine()
                .AppendLine()
                .Append(Templates.HttpMethodAttribute(verb));
        }

        context.AddSource("Interfaces.g.cs", sb.ToString());
    }


    private static void Execute((Compilation Left, ImmutableArray<List<Endpoint>> Right) source, SourceProductionContext spc, string implementingMethod, string hintName)
    {
        StringBuilder sb = new(1024);

        (Compilation compilation, ImmutableArray<List<Endpoint>> endpointsPerClass) = source;

        List<Endpoint> endpoints = [];

        foreach (var item in endpointsPerClass)
        {
            endpoints.AddRange(item);
        }


        var hasGroup = endpoints is { Count: > 0 } && endpoints[0].Group is not null;

        sb.Append($$"""
            // <auto-generated />
            using Microsoft.AspNetCore.Builder;
            using Microsoft.AspNetCore.Http;

            namespace Nocpad.AspNetCore.MinimalEndpoints;

            public static partial class MapMinimalEndpointsExtensions
            {
                public static partial WebApplication {{implementingMethod}}(this WebApplication app)
                {
  
            """);

        if (!hasGroup)
        {
            AddEndpointsWithoutGroup(endpoints, "app", "endpoint", sb);
        }
        else
        {
            var groupedEndpoints = endpoints.GroupBy(e => e.Group?.Name);
            var groupNames = groupedEndpoints.Select(g => g.Key).ToList();

            int groupCount = 1;
            int endpointCount = 1;
            foreach (var group in groupedEndpoints)
            {
                var groupEndpoints = group.ToList();

                var groupName = $"group{groupCount++}";

                sb.Append("        ")
                    .AppendLine($"var {groupName} = app.MapGroup(\"{groupEndpoints[0].Group!.Route}\").WithTags(\"{groupEndpoints[0].Group!.Name}\");");

                if (groupEndpoints[0].Group is { HasConfiguration: true })
                {
                    sb.Append("        ")
                        .AppendLine($"{groupEndpoints[0].Group!.Namespace}.Configure({groupName});");
                }

                AddEndpointsWithoutGroup(groupEndpoints, groupName, $"{groupName}Endpoint", sb);
            }
        }

        sb.Append("""
                    return app;
                }
            }
            """);

        spc.AddSource($"MapMinimalEndpointsExtensions.{hintName}.g.cs", sb.ToString());
    }


    private static void AddEndpointsWithoutGroup(List<Endpoint> endpoints, string parent, string variablePrefix, StringBuilder sb)
    {
        int counter = 1;
        foreach (var item in endpoints)
        {
            if (!item.Config.Active)
            {
                continue;
            }

            var endpointVariableName = $"{variablePrefix}{counter++}";

            sb.AppendLine();
            item.AppenEndpointString(sb, parent, endpointVariableName);

            sb.AppendLine();
        }
    }


    internal static List<Endpoint> GetSemanticTargetForGeneration(GeneratorAttributeSyntaxContext context, CancellationToken ct)
    {
        if (context.TargetSymbol is not INamedTypeSymbol classSymbol)
        {
            return [];
        }

        ct.ThrowIfCancellationRequested();

        var classMembers = classSymbol.GetMembers();

        List<Endpoint> endpoints = [];

        foreach (var member in classMembers)
        {
            if (member is not IMethodSymbol method)
            {
                continue;
            }

            var attributes = method.GetAttributes();

            foreach (var attribute in attributes)
            {
                var httpMethod = $"{attribute.AttributeClass.ContainingNamespace.ToDisplayString()}.{attribute.AttributeClass.Name}" switch
                {
                    "Nocpad.AspNetCore.MinimalEndpoints.DeleteAttribute" => "DELETE",
                    "Nocpad.AspNetCore.MinimalEndpoints.GetAttribute" => "GET",
                    "Nocpad.AspNetCore.MinimalEndpoints.PostAttribute" => "POST",
                    "Nocpad.AspNetCore.MinimalEndpoints.PutAttribute" => "PUT",
                    _ => null
                };

                if (httpMethod is null)
                {
                    continue;
                }

                var classDeclarationSyntax = (ClassDeclarationSyntax)context.TargetNode;

                var endpointAttribute = classDeclarationSyntax.AttributeLists[0].Attributes[0];

                var validateAttribute = attributes.FirstOrDefault(e => e.AttributeClass?.MetadataName == "Validate`1" && e.AttributeClass.IsGenericType);

                EndpointConfig config = new()
                {
                    Active = GetAttributeNamedArgument<bool?>(endpointAttribute, nameof(EndpointConfig.Active)) != false,
                    RequireAuthorization = GetNamedArgumentValueOrDefault<bool?>(attribute, nameof(EndpointConfig.RequireAuthorization)),
                    Policies = GetNamedArgumentValuesOrDefault<string>(attribute, nameof(EndpointConfig.Policies)),
                    Validator = validateAttribute is null
                        ? null
                        : new(validateAttribute.AttributeClass!.TypeArguments[0].ContainingNamespace, validateAttribute.AttributeClass.TypeArguments[0].Name)
                };

                endpoints.Add(new Endpoint
                {
                    Config = config,
                    HasConfiguration = classSymbol.AllInterfaces.Any(e => $"{e.ContainingNamespace.ToDisplayString()}.{e.Name}" == EndpointWithConfiguration),
                    EndpointMethod = member.Name,
                    HttpMethod = httpMethod,
                    Namespace = new(method.ContainingNamespace, classSymbol.Name),
                    Template = attribute.ConstructorArguments[0].Value?.ToString()!,
                    Group = TryGetGroup(endpointAttribute, context),
                });
            }
        }

        return endpoints;
    }

    internal static T GetNamedArgumentValueOrDefault<T>(AttributeData attribute, string name)
    {
        var value = attribute.NamedArguments.FirstOrDefault(e => e.Key == name);
        return value is { }
            ? (T)value.Value.Value!
            : default;
    }

    internal static T[]? GetNamedArgumentValuesOrDefault<T>(AttributeData attribute, string name)
    {
        var value = attribute.NamedArguments.FirstOrDefault(e => e.Key == name);
        return value is { Value.Kind: TypedConstantKind.Array, Value.IsNull: false }
            ? value.Value.Values.Select(e => (T)e.Value!).ToArray()
            : default;
    }


    internal static T? GetAttributeNamedArgument<T>(AttributeSyntax attribute, string name)
    {
        var argument = attribute.ArgumentList?.Arguments.FirstOrDefault(arg => arg.NameEquals?.Name.Identifier.Text == name);
        return argument?.Expression is LiteralExpressionSyntax literalExpression
            ? (T)literalExpression.Token.Value!
            : default;
    }


    internal static EndpointGroup? TryGetGroup(AttributeSyntax endpointAttribute, GeneratorAttributeSyntaxContext context)
    {
        EndpointGroup? endpointGroup = null;

        if (endpointAttribute.Name is GenericNameSyntax genericNameSyntax)
        {
            endpointGroup = new();

            // TODO: if there are more than 1 class endpoint group per file this will pick the first one
            var typeInfo = context.SemanticModel.GetTypeInfo(genericNameSyntax.TypeArgumentList.Arguments[0]).Type!;
            var syntaxTree = typeInfo.Locations.First().SourceTree;
            var root = syntaxTree!.GetRoot();

            endpointGroup.Namespace = new(typeInfo!.ContainingNamespace, typeInfo.Name);

            var groupMembers = typeInfo.GetMembers();

            endpointGroup.Name = TryGetGroupPropertyValue(root, context.SemanticModel.Compilation, groupMembers, nameof(EndpointGroup.Name))!;
            endpointGroup.Route = TryGetGroupPropertyValue(root, context.SemanticModel.Compilation, groupMembers, nameof(EndpointGroup.Route))!;
            endpointGroup.HasConfiguration = typeInfo.AllInterfaces.Any(e => $"{typeInfo.AllInterfaces[0].ContainingNamespace.ToDisplayString()}.{e.Name}" == EndpointGroupWithConfiguration);
        }

        return endpointGroup;
    }

    internal static string? TryGetGroupPropertyValue(SyntaxNode node, Compilation compilation, ImmutableArray<ISymbol> groupMembers, string propertyName)
    {
        foreach (var m in groupMembers)
        {
            if (m is not IPropertySymbol propertySymbol)
            {
                continue;
            }

            if (propertySymbol.Name == propertyName)
            {
                var propertyDeclaration = node.DescendantNodes().OfType<PropertyDeclarationSyntax>().FirstOrDefault(p => p.Identifier.Text == propertySymbol.Name);

                if (propertyDeclaration.Initializer?.Value is not null)
                {
                    return GetInitializerPropertyValue<string>(compilation, propertyDeclaration, propertyDeclaration.Initializer.Value)!;
                }
                else if (propertyDeclaration.ExpressionBody is not null)
                {
                    return GetInitializerPropertyValue<string>(compilation, propertyDeclaration, propertyDeclaration.ExpressionBody.Expression)!;
                }
                else
                {
                    // TODO: don't know
                }
            }
        }

        return null;
    }


    internal static T? GetInitializerPropertyValue<T>(Compilation compilation, PropertyDeclarationSyntax? propertyDeclaration, ExpressionSyntax expression)
    {
        // Evaluate the initializer expression
        var initializerValue = compilation.GetSemanticModel(propertyDeclaration.SyntaxTree).GetConstantValue(expression);

        if (initializerValue.HasValue && initializerValue.Value is T value)
        {
            return value;
        }
        else
        {
            // TODO: warning 
            return default;
        }
    }


    internal static void RegisterPostInitialization(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(async ctx =>
        {
            var httpAttriteTempate = await GetSourceFileTemplate("HttpMethodAttribute.template", null);
            string[] verbs = ["Get", "Post", "Put", "Delete"];

            foreach (var verb in verbs)
            {
                ctx.AddSource($"{verb}Attribute.g.cs", SourceText.From(httpAttriteTempate.Replace("{{METHOD}}", verb), Encoding.UTF8));
            }

            ctx.AddSource("EndpointInterfaces.g.cs", SourceText.From(await GetSourceFileTemplate("Interfaces.template", null), Encoding.UTF8));
            ctx.AddSource("EndpointAttribute.g.cs", SourceText.From(await GetSourceFileTemplate("EndpointAttribute.template", null), Encoding.UTF8));

            ctx.AddSource("MapMinimalEndpointsExtensions.g.cs", $$"""
                // <auto-generated />
                using Microsoft.AspNetCore.Builder;

                public static partial class MapMinimalEndpointsExtensions
                {
                    public static WebApplication MapMinimalEndpoints(this WebApplication app)
                    {
                        return app
                            .MapIndividualEndpoints()
                            .MapEndpointGroups();
                    }

                    public static partial WebApplication MapIndividualEndpoints(this WebApplication app);

                    public static partial WebApplication MapEndpointGroups(this WebApplication app);
                } 
                """);
        });
    }


    internal static async Task<string> GetSourceFileTemplate(string resourceName, string outputNamespace)
    {
        var assembly = Assembly.GetExecutingAssembly();
        string resourcePath = assembly
            .GetManifestResourceNames()
            .FirstOrDefault(e => e.Contains(resourceName));

        using Stream stream = assembly.GetManifestResourceStream(resourcePath);
        using StreamReader reader = new(stream);

        var content = await reader.ReadToEndAsync();

        // TODO: allow overriding the namespace of each source file
        return content.Replace("{{GeneratorNamespace}}", outputNamespace ?? "Nocpad.AspNetCore.MinimalEndpoints");
    }
}



internal static class Templates
{
    internal static string HttpMethodAttribute(string method) => $$"""
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
        public sealed class {{method}}Attribute : Attribute
        {
            public {{method}}Attribute(string template) => Template = template;

            public bool RequireAuthorization { get; set; }

            public string[]? Policies { get; set; }

            public string Template { get; }
        }
        """;
}

internal sealed record GeneratorOptions
{
    public bool HasFluentValidationSupport { get; set; }
}