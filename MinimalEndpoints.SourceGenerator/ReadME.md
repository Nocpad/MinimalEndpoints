
# About
A source generator that can be used along with the `Nocpad.AspNetCore.MinimalEndpoints` package to automatically generate the boilerplate code required to map endpoints.

# Examples

### Endpoint associated with a group 
```c#
using Nocpad.AspNetCore.MinimalEndpoints; // you may add a global using statement

internal sealed class WeatherEndpointGroup : IEndpointGroupConfiguration
{
    public static string Route => "api/weather";

    public static string Name => "Weather";

    public static void Configure(RouteGroupBuilder group) => group.RequireAuthorization();
}


internal sealed class GetWeather : IEndpointConfiguration<WeatherGroup>
{
    internal static void Configure(RouteHandlerBuilder builder)
    {
        // endpoint configuration
    }

    [Get("forecast")]
    internal static WeatherForecast[] Get()
    {
       // implementation...
    }
}
```


### Simple endpoint without a group/configuration

```c#
internal sealed class Upload : IEndpoint
{
    [Post("/file-upload")]
    public static IResult Handle(IFormFile file) => Results.Ok(new { file.FileName, file.ContentType, file.Length });
}
```