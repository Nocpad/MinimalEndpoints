namespace Nocpad.AspNetCore.MinimalEndpoints;

public sealed class PutAttribute(string template, string method = "PUT") : HttpMethodAttribute(template, method) { }
