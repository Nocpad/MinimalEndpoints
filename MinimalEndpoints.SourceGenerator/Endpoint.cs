﻿using Microsoft.CodeAnalysis;
using System.Text;

namespace MinimalEndpoints.SourceGenerator;

internal sealed record Endpoint
{
    public bool RequireServiceRegistration => !Method.IsStatic;

    public string ClassName { get; set; } = null!;

    public bool IsStaticClass { get; set; }

    public EndpointConfig Config { get; set; } = null!;

    public IMethodSymbol Method { get; set; } = null!;

    public string Template { get; set; } = string.Empty;

    public string HttpMethod { get; set; } = string.Empty;

    public EndpointNamespace Namespace { get; set; } = null!;

    public bool HasConfiguration { get; set; }

    public string ConfigureMethodName { get; set; } = null!;

    public EndpointGroup? Group { get; set; }


    public void AppendEndpointString(StringBuilder sb, string endpointParent, string variableName)
    {
        sb.Append("        ")
            .Append($"var {variableName} = {endpointParent}");

        if (Method.IsStatic is true)
        {
            sb.Append($".MapMethods(\"{Template}\", new[] {{\"{HttpMethod}\"}}, {Method.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)})");
        }
        else
        {
            var asyncKeyword = Method.ReturnType.ToDisplayString().StartsWith(TaskBaseType) ? "async" : null;

            var handlersSignature = Method.Parameters.Length is > 0
                ? string.Join(", ", Method.Parameters.Select(p => $"{p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {p.Name}")) + ", " // also include the comma 
                : string.Empty;


            sb.Append($$"""
                .MapMethods("{{Template}}", new[] {"{{HttpMethod}}"}, {{asyncKeyword}} ({{handlersSignature}} {{ClassName}} handler) =>
                        {
                            {{HandlerCall()}}
                        })
                """);
        }


        if (Config.Validator is { })
        {
            sb.AppendLine()
                .Append("        ")
                .Append("        ")
                .Append($".AddEndpointFilter<AbstractValidationFilter<{Config.Validator}>>()");
        }

        if (Config.RequireAuthorization is not null)
        {
            sb.AppendLine()
                .Append("        ")
                .Append("        ")
                .Append(Config.RequireAuthorization == true ? ".RequireAuthorization()" : ".AllowAnonymous()");
        }

        if (Config.Policies is { Length: > 0 })
        {
            sb.AppendLine()
                .Append("        ")
                .Append("        ")
                .Append($$""".RequireAuthorization("{{string.Join("\", \"", Config.Policies)}}")""");
        }

        sb.Append(";");

        if (HasConfiguration)
        {
            sb.AppendLine()
                .Append("        ")
                .Append($"{Namespace}.{ConfigureMethodName}({variableName});");
        }

        sb.AppendLine();
    }


    const string TaskBaseType = "System.Threading.Tasks.Task";

    private string HandlerCall()
    {
        if (Method.ReturnsVoid)
        {
            return CreateHandlerCallString();
        }
        else
        {
            var returnType = Method.ReturnType.ToDisplayString();

            bool isTask = returnType.StartsWith(TaskBaseType);

            bool hasTaskResult = isTask && returnType.Length > TaskBaseType.Length && returnType[TaskBaseType.Length] == '<' && returnType.EndsWith(">");

            if (isTask)
            {
                return hasTaskResult
                    ? $"return await {CreateHandlerCallString()}"
                    : $"await {CreateHandlerCallString()}";
            }

            return $"return {CreateHandlerCallString()}";
        }


        string CreateHandlerCallString()
        {
            return $"handler.{Method.Name}({string.Join(", ", Method.Parameters.Select(e => e.Name))});";
        }
    }
}
