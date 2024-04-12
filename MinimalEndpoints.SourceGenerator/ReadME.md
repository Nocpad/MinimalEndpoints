
# About
A source generator that can be used to automatically generate the boilerplate code required to map endpoints.

# Examples

### Program.cs

```c#
app.MapMinimalEndpoints();
```


### Endpoint associated with a group 
```c#
using Nocpad.AspNetCore.MinimalEndpoints; // you may add a global using statement

internal sealed class WeatherEndpointGroup : IEndpointGroupConfiguration
{
    public static string Route => "api/weather";

    public static string Name => "Weather";

    public static void Configure(RouteGroupBuilder group) => group.RequireAuthorization();
}


[Endpoint<WeatherEndpointGroup>]
internal sealed class GetWeather : IEndpoint
{
    [Get("forecast")]
    // [Validate<Request>, Get("forecast")] 
    // [Validate<Request>, Get("forecast", Policies = ["policy name"])
    // [Validate<Request>, Get("forecast", Policies = ["policy name", "second policy"], RequireAuthorization = true/false)]
    internal static WeatherForecast[] Get(Request request)
    {
       // implementation...
    }
}
```


### Simple endpoint without a group/configuration

```c#
[Endpoint]
internal sealed class Upload
{
    [Post("/file-upload")]
    public static IResult Handle(IFormFile file) => Results.Ok(new { file.FileName, file.ContentType, file.Length });
}
```

```c#
[Endpoint]
internal sealed class Upload : IEndpointConfiguration
{
    public static void Configure(RouteHandlerBuilder builder)
    {
        // endpoint configuration
    }

    [Post("/file-upload")]
    public static IResult Handle(IFormFile file) => Results.Ok(new { file.FileName, file.ContentType, file.Length });
}
```