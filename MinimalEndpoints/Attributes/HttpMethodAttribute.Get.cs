namespace Nocpad.AspNetCore.MinimalEndpoints;

public sealed class GetAttribute(string template, string method = "GET") : HttpMethodAttribute(template, method) { }
