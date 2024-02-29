namespace MinimalEndpoints.SourceGenerator;

internal record EndpointConfig
{
    public bool Active { get; set; }

    public bool? RequireAuthorization { get; set; }

    public string[]? Policies { get; set; }


    public override string ToString()
    {
        string result = string.Empty;

        if (RequireAuthorization is not null)
        {
            result += RequireAuthorization.Value
                ? ".RequireAuthorization()"
                : ".AllowAnonymous()";
        }

        if (Policies is not null)
        {
            result += $$""".RequireAuthorization("{{string.Join("\", \"", Policies)}}")""";
        }

        return result;
    }
}
