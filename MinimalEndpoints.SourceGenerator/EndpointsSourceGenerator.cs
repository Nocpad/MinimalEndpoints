//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.CSharp.Syntax;
//using Microsoft.CodeAnalysis.Text;
//using MinimalEndpoints.SourceGenerator.Models;
//using System.Collections.Generic;
//using System.Collections.Immutable;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;

//namespace MinimalEndpoints.SourceGenerator
//{
//    [Generator]
//    public sealed class EndpointsSourceGenerator : ISourceGenerator
//    {
//        const string AbstractValidationFilter = "AbstractValidationFilter";
//        const string AbstractValidationFilterNamespace = "Nocpad.AspNetCore.MinimalEndpoints.Validations";


//        const string BaseInterfaceName = "IEndpoint";
//        const string BaseConfigurationInterfaceName = "IEndpointConfiguration";
//        const string EndpointInterfaceName = "Nocpad.AspNetCore.MinimalEndpoints.IEndpoint";
//        const string HttpBaseAttributeSymbol = "Nocpad.AspNetCore.MinimalEndpoints.HttpMethodAttribute";
//        const string AllowMultipleEndpointsAttributeName = "Nocpad.AspNetCore.MinimalEndpoints.AllowMultipleEndpointsAttribute";
//        const string EndpointConfigurationInterfaceName = "Nocpad.AspNetCore.MinimalEndpoints.IEndpointConfiguration";
//        const string EndpointGroupConfigurationInterfaceName = "Nocpad.AspNetCore.MinimalEndpoints.IEndpointGroupConfiguration";
//        const string ValidateAttributeName = "Nocpad.AspNetCore.MinimalEndpoints.Validations.ValidateAttribute`1"; // TODO: 

//        INamedTypeSymbol? interfaceSymbol;
//        INamedTypeSymbol? baseAttributeSymbol;
//        INamedTypeSymbol? allowMultipleEndpointsAttributeSymbol;
//        INamedTypeSymbol? endpointConfigurationSymbol;
//        INamedTypeSymbol? endpointGroupConfigurationSymbol;
//        INamedTypeSymbol? validateAttributeSymbol;
//        GeneratorExecutionContext _context;
//        EndpointDefinitions _endpointDefinitions;

//        public void Execute(GeneratorExecutionContext context)
//        {
//            Compilation compilation = context.Compilation;
//            _context = context;

//            // Get the interface symbol
//            interfaceSymbol = TryGetTypeSymbol(context, EndpointInterfaceName);
//            baseAttributeSymbol = TryGetTypeSymbol(context, HttpBaseAttributeSymbol);
//            endpointConfigurationSymbol = TryGetTypeSymbol(context, EndpointConfigurationInterfaceName);
//            endpointGroupConfigurationSymbol = TryGetTypeSymbol(context, EndpointGroupConfigurationInterfaceName);
//            endpointGroupConfigurationSymbol = TryGetTypeSymbol(context, EndpointGroupConfigurationInterfaceName);
//            allowMultipleEndpointsAttributeSymbol = TryGetTypeSymbol(context, AllowMultipleEndpointsAttributeName);
//            validateAttributeSymbol = TryGetTypeSymbol(context, ValidateAttributeName, false);


//            _endpointDefinitions = new()
//            {
//                RootNamespace = context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.rootnamespace", out var rootNamespace) ? rootNamespace : null,
//                HasValidationSupport = validateAttributeSymbol is not null,
//            };


//            if (interfaceSymbol is null ||
//                  baseAttributeSymbol is null ||
//                  endpointConfigurationSymbol is null ||
//                  endpointGroupConfigurationSymbol is null)
//                return;

//            foreach (SyntaxTree syntaxTree in compilation.SyntaxTrees)
//            {
//                SemanticModel semanticModel = compilation.GetSemanticModel(syntaxTree);

//                foreach (ClassDeclarationSyntax classSyntax in syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>())
//                {
//                    ITypeSymbol classSymbol = semanticModel.GetDeclaredSymbol(classSyntax) as ITypeSymbol;

//                    if (!classSymbol.AllInterfaces.Contains(interfaceSymbol)) continue;


//                    var endpoints = GetClassEndpoints(compilation, classSyntax, semanticModel, classSymbol);

//                    if (endpoints is { Count: 0 })
//                    {
//                        context.AddDiagnostic(DiagnosticCodes.ClassWithNoEndpointMethod, DiagnosticSeverity.Warning, classSymbol.Locations.FirstOrDefault());
//                    }
//                    else if (endpoints is { Count: > 1 } && endpoints[0].HasOwnConfiguration)
//                    {
//                        var hasSuppressedError = classSymbol.GetAttributes().Any(a => a.AttributeClass.Equals(allowMultipleEndpointsAttributeSymbol));

//                        if (!hasSuppressedError)
//                        {
//                            context.AddDiagnostic(DiagnosticCodes.ClassWithMultipleEndpoints, DiagnosticSeverity.Error, classSymbol.Locations.FirstOrDefault());
//                        }
//                    }

//                    endpoints.ForEach(_endpointDefinitions.AddEndpoint);
//                }
//            }

//            GenerateEndpointsSource(context, _endpointDefinitions);
//        }


//        INamedTypeSymbol? TryGetTypeSymbol(GeneratorExecutionContext context, string typeName, bool reportIfNotFound = true)
//        {
//            var symbol = context.Compilation.GetTypeByMetadataName(typeName);

//            if (symbol is null && reportIfNotFound)
//            {
//                context.AddDiagnostic(DiagnosticCodes.SymbolNotFound, DiagnosticSeverity.Warning, args: typeName);
//            }

//            return symbol;
//        }


//        private List<Endpoint> GetClassEndpoints(Compilation compilation, ClassDeclarationSyntax classSyntax, SemanticModel semanticModel, ITypeSymbol classSymbol)
//        {
//            List<Endpoint> endpoints = new();

//            foreach (MethodDeclarationSyntax methodSyntax in classSyntax.Members.OfType<MethodDeclarationSyntax>())
//            {
//                IMethodSymbol methodSymbol = (IMethodSymbol)semanticModel.GetDeclaredSymbol(methodSyntax)!;

//                var methodAttributes = methodSymbol.GetAttributes();

//                AttributeData? attributeData = methodAttributes.FirstOrDefault(attr => IsDerivedFromAttribute(attr.AttributeClass, baseAttributeSymbol));

//                if (attributeData is null)
//                {
//                    continue;
//                }

//                if (!methodSymbol.IsStatic)
//                {
//                    _context.AddDiagnostic(DiagnosticCodes.NonStaticMethod, DiagnosticSeverity.Error, methodSymbol.Locations.FirstOrDefault());
//                }

//                string template = GetEndpointTemplate(attributeData);
//                string method = GetEndpointMethod(attributeData);
//                var policies = GetEndpointPolicies<string>(attributeData, "Policies");
//                var requireAuthorization = GetNamedAttributeValue<bool?>(attributeData, "RequireAuthorization");
//                var epHasOwnConfiguration = classSymbol.AllInterfaces.Contains(endpointConfigurationSymbol);

//                var validators = _endpointDefinitions.HasValidationSupport
//                    ? GetEndpointValidators(methodAttributes)
//                    : null;

//                var endpoint = new Endpoint
//                {
//                    Namespace = new Namespace()
//                    {
//                        IsGlobalNamespace = classSymbol.ContainingNamespace.IsGlobalNamespace,
//                        Name = classSymbol.ContainingNamespace.IsGlobalNamespace ? "global::" : classSymbol.ContainingNamespace.ToDisplayString()
//                    },
//                    Class = GetClassName(classSymbol),
//                    MethodName = methodSymbol.Name,
//                    HasOwnConfiguration = epHasOwnConfiguration,
//                    Template = template,
//                    HttpMethod = method,
//                    Policies = policies,
//                    RequireAuthorization = requireAuthorization,
//                    Group = GetGroupInfo(compilation, classSymbol),
//                    Validators = validators
//                };

//                endpoints.Add(endpoint);
//            }
//            return endpoints;
//        }

//        private List<Validator> GetEndpointValidators(ImmutableArray<AttributeData> methodAttributes) => methodAttributes
//            .Where(e => e.AttributeClass.ConstructedFrom.Equals(validateAttributeSymbol))
//            .Select(e => new Validator
//            {
//                Class = GetClassName(e.AttributeClass!.TypeArguments[0]),
//                Namespace = e.AttributeClass.TypeArguments[0].ContainingNamespace.ToDisplayString(),
//            }).ToList();


//        private string GetClassName(ITypeSymbol symbol)
//        {
//            List<string> names = [symbol.Name];

//            var containingType = symbol.ContainingType;

//            while (containingType is not null)
//            {
//                names.Add(containingType.Name);
//                containingType = containingType.ContainingType;
//            }

//            names.Reverse();
//            return string.Join(".", names);
//        }


//        private RouteGroup? GetGroupInfo(Compilation compilation, ITypeSymbol classSymbol)
//        {
//            var groupInterfaceSymbol = classSymbol.AllInterfaces.FirstOrDefault(e => e.Name is BaseInterfaceName or BaseConfigurationInterfaceName);

//            if (groupInterfaceSymbol is null or { TypeArguments.Length: 0 }) return null;

//            var genericTypeArgument = groupInterfaceSymbol.TypeArguments[0];

//            if (genericTypeArgument is null)
//            {
//                // TODO: warning
//                return default;
//            }

//            var groupPrefix = GetGroupPropertyValue<string>(compilation, genericTypeArgument, "Route");

//            if (groupPrefix is null) return null;

//            var groupName = GetGroupPropertyValue<string>(compilation, genericTypeArgument, "Name");

//            return new()
//            {
//                Class = GetClassName(genericTypeArgument),
//                Namespace = genericTypeArgument.ContainingNamespace.ToDisplayString(),
//                Prefix = groupPrefix,
//                Name = groupName!,
//                HasOwnConfiguration = genericTypeArgument.AllInterfaces.Contains(endpointGroupConfigurationSymbol!)
//            };
//        }


//        private static T? GetGroupPropertyValue<T>(Compilation compilation, ITypeSymbol groupSymbol, string property)
//        {
//            // Find the property symbol
//            var propertySymbol = groupSymbol
//                .GetMembers()
//                .OfType<IPropertySymbol>()
//                .FirstOrDefault(p => p.IsStatic && p.Type.SpecialType == SpecialType.System_String && p.Name == property);

//            if (propertySymbol is null)
//            {
//                // TODO: warning
//                return default;
//            }



//            if (propertySymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is not PropertyDeclarationSyntax propertyDeclaration)
//            {
//                // TODO: warning
//                return default;
//            }


//            if (propertyDeclaration.Initializer?.Value is not null)
//            {
//                return GetInitializerPropertyValue<T>(compilation, propertyDeclaration, propertyDeclaration.Initializer.Value);
//            }
//            else if (propertyDeclaration.ExpressionBody is not null)
//            {
//                return GetInitializerPropertyValue<T>(compilation, propertyDeclaration, propertyDeclaration.ExpressionBody.Expression);
//            }
//            else
//            {
//                // TODO: can't resolve
//                return default;
//            }

//        }

//        private static T? GetInitializerPropertyValue<T>(Compilation compilation, PropertyDeclarationSyntax? propertyDeclaration, ExpressionSyntax expression)
//        {
//            // Evaluate the initializer expression
//            var initializerValue = compilation.GetSemanticModel(propertyDeclaration.SyntaxTree).GetConstantValue(expression);

//            if (initializerValue.HasValue && initializerValue.Value is T value)
//            {
//                return value;
//            }
//            else
//            {
//                // TODO: warning 
//                return default;
//            }
//        }


//        private bool IsDerivedFromAttribute(INamedTypeSymbol attributeType, INamedTypeSymbol baseAttributeType) => attributeType != null && (attributeType.Equals(baseAttributeType) || IsDerivedFromAttribute(attributeType.BaseType, baseAttributeType));


//        private T? GetNamedAttributeValue<T>(AttributeData attributeData, string propertyName)
//        {
//            TypedConstant propertyValue = attributeData.NamedArguments.FirstOrDefault(arg => arg.Key == propertyName).Value;

//            if (propertyValue.Value is null) return default;

//            return propertyValue.Value != null ? (T)propertyValue.Value : default;
//        }


//        private T[]? GetEndpointPolicies<T>(AttributeData attributeData, string propertyName)
//        {
//            TypedConstant propertyValue = attributeData.NamedArguments.FirstOrDefault(arg => arg.Key == propertyName).Value;

//            if (propertyValue.Kind == TypedConstantKind.Array)
//            {
//                return propertyValue.Values.Select(e => (T)e.Value).ToArray();
//            }

//            return default;
//        }


//        private string GetEndpointTemplate(AttributeData attributeData) => attributeData.ConstructorArguments.Length is 0 ? null : (string)attributeData.ConstructorArguments[0].Value;


//        private string GetEndpointMethod(AttributeData attributeData) => attributeData.ConstructorArguments.Length is 0 ? null : (string)attributeData.ConstructorArguments[1].Value;


//        private void GenerateEndpointsSource(GeneratorExecutionContext context, EndpointDefinitions definitions)
//        {
//            StringBuilder sb = new();
//            int counter = 1;

//            sb.Append($$"""
//            using Microsoft.AspNetCore.Builder;
//            using Microsoft.AspNetCore.Routing;
//            {{(definitions.HasValidationSupport ? $"using {AbstractValidationFilterNamespace};" : null)}}

//            namespace {{definitions.RootNamespace}};

//            public static class MinimalEndpointsExtensions
//            {
//                public static WebApplication MapMinimalEndpoints(this WebApplication app)
//                {
                    
//            """);

//            var endpoints = definitions.Endpoints;
//            foreach (var endpoint in endpoints)
//            {
//                MapEndpoint(endpoint, sb, counter);
//                counter++;
//            }

//            GenerateGroupEndpoints(definitions.Groups, sb);

//            sb.Append("""


//                    return app;
//                }
//            }
//            """);

//            var source = sb.ToString();
//            var sourceText = SourceText.From(source, Encoding.UTF8);
//            context.AddSource("MinimalEndpoints.Generated.cs", sourceText);
//        }


//        private static void MapEndpoint(Endpoint endpoint, StringBuilder sb, int counter)
//        {
//            sb.Append($$"""var ep{{counter}} = app.MapMethods("{{endpoint.Template}}", new[] { "{{endpoint.HttpMethod}}" }, {{endpoint.Namespace}}{{endpoint.Class}}.{{endpoint.MethodName}})""");

//            if (endpoint.RequireAuthorization == true)
//            {
//                sb.Append(".RequireAuthorization()");
//            }

//            if (endpoint.Policies is not null and { Length: > 0 })
//            {
//                var policies = string.Join(",", endpoint.Policies.Select(e => @$"""{e}"""));
//                sb.Append($".RequireAuthorization( {policies} )");
//            }

//            if (endpoint.Validators is { Count: > 0 })
//            {
//                foreach (var validator in endpoint.Validators)
//                {
//                    sb.AppendLine().Append($"              .AddEndpointFilter<{AbstractValidationFilter}<{validator.Namespace}.{validator.Class}>>()");

//                }
//            }

//            sb.Append(";").AppendLine("        ");

//            if (endpoint.HasOwnConfiguration)
//            {
//                sb.Append($$"""
//                        {{endpoint.Namespace}}{{endpoint.Class}}.Configure(ep{{counter}});
//                """).AppendLine().Append("        ");
//            }

//            sb.AppendLine().Append("        ");
//        }


//        private static void GenerateGroupEndpoints(List<EndpointsGroup> groups, StringBuilder sb)
//        {
//            int groupCount = 1;
//            int endpointCount = 1;

//            foreach (var group in groups)
//            {
//                string groupName = $"group{groupCount++}";

//                sb.Append($$"""
//                        var {{groupName}} = app.MapGroup("{{group.Prefix}}").WithTags("{{group.Name}}");
//                            {{(group.HasOwnConfiguration ? $$"""{{group.Namespace}}.{{group.Class}}.Configure({{groupName}});""" : "")}}

//                """);


//                foreach (var endpoint in group.Endpoints)
//                {
//                    string endpointName = $"{groupName}Endpoint{endpointCount++}";

//                    sb.Append($$"""

//                            var {{endpointName}} = {{groupName}}.MapMethods("{{endpoint.Template}}", new[]{ "{{endpoint.HttpMethod}}" }, {{endpoint.Namespace}}{{endpoint.Class}}.{{endpoint.MethodName}})
//                    """);

//                    if (endpoint.RequireAuthorization == true) sb.AppendLine().Append("              .RequireAuthorization()");
//                    if (endpoint.RequireAuthorization == false) sb.AppendLine().Append("              .AllowAnonymous()");
//                    if (endpoint.Policies is { Length: > 0 })
//                    {
//                        var policies = string.Join(",", endpoint.Policies.Select(e => @$"""{e}"""));
//                        sb.AppendLine().Append($"              .RequireAuthorization({policies})");
//                    }

//                    if (endpoint.Validators is { Count: > 0 })
//                    {
//                        foreach (var validator in endpoint.Validators)
//                        {
//                            sb.AppendLine().Append($"              .AddEndpointFilter<{AbstractValidationFilter}<{validator.Namespace}.{validator.Class}>>()");

//                        }
//                    }

//                    sb.Append(";").AppendLine();

//                    if (endpoint.HasOwnConfiguration)
//                    {
//                        sb.Append($$"""
//                                    {{endpoint.Namespace}}{{endpoint.Class}}.Configure({{endpointName}});

//                            """);
//                    }
//                }
//            }
//        }

//        public void Initialize(GeneratorInitializationContext context)
//        {
//#if DEBUG
//            if (!Debugger.IsAttached)
//            {
//                Debugger.Launch();
//            }
//#endif

//            Debug.WriteLine("Initialize code generator");
//        }
//    }
//}
