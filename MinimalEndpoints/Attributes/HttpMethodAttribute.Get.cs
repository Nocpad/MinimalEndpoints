using System;

namespace Nocpad.AspNetCore.MinimalEndpoints;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class GetAttribute(string template) : Attribute
{
    public bool RequireAuthorization { get; set; }

    public string[]? Policies { get; set; }
}
