namespace Nocpad.AspNetCore.MinimalEndpoints;

public sealed class PostAttribute(string template, string method = "POST") : HttpMethodAttribute(template, method) { }
