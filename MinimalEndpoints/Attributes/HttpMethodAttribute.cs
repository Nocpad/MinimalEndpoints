using System;

namespace Nocpad.AspNetCore.MinimalEndpoints;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class HttpMethodAttribute : Attribute
{
    public HttpMethodAttribute(string template, string method) => (Template, Method) = (template, method);

    public string Template { get; }

    public string Method { get; }

    public bool RequireAuthorization { get; set; }

    public string[]? Policies { get; set; }
}
