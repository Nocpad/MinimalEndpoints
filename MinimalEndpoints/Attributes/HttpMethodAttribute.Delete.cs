namespace Nocpad.AspNetCore.MinimalEndpoints;

public sealed class DeleteAttribute(string template, string method = "DELETE") : HttpMethodAttribute(template, method) { }