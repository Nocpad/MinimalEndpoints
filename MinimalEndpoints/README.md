
# About
This package contains marker interfaces used by the `Nocpad.AspNetCore.MinimalEndpoints.SourceGenerator` to define and automatically register minimal API endpoints.


- [Usage](#usage)
- [Endpoint groups examples](#group-examples)
- [Endpoint examples](#endpoint-examples)
- [Endpoints with groups](#grouped-endpoints)
- [Attributes](#attributes)

---
## Usage {#usage}

Add this line on `Program.cs` to map the discovered endpoints.
```c# 
app.MapNocpad.AspNetCore.MinimalEndpoints();
```

>**`The MapNocpad.AspNetCore.MinimalEndpoints() extension method is under the root namespace so you may need to add the using statement.`**

---
## Endpoint groups examples  {#group-examples}

You can create endpoint groups in order to add common behavior to the included endpoints by implementing the `IEndpointGroup` interface.

```c#
public class WeatherEndpointGroup : IEndpointGroup
{
    public static string Route => "weather";

    public static string Name => "Weather";
}
```

If any configuration is required you can implement `IEndpointGroupConfiguration` instead and configure the route group on the `Configure` method.
```c#
public class WeatherEndpointGroup : IEndpointGroupConfiguration
{
    public static string Route => "api/weather";

    public static string Name => "Weather";

    public static void Configure(RouteGroupBuilder group) => group.RequireAuthorization();
}
```

---
## Endpoint examples  {#endpoint-examples}

You can expose methods as endpoints by implementing `IEndpoint` or `IEndpointConfiguration` interface  
and decorate the method using one of the [attributes](#attributes).

>Currently only static methods are supported, the source generator will warn you by emitting the error `Non static endpoints methods are not supported`


```c#
internal sealed class Upload : IEndpoint
{
    [Post("/file-upload")]
    public static IResult Handle(IFormFile file) => Results.Ok(new { file.FileName, file.ContentType, file.Length });
}
```

```c#
public sealed class CreateForecast : IEndpointConfiguration
{
    public static void Configure(RouteHandlerBuilder builder) => builder
        .WithSummary("Summary")
        .WithName("CreateForecast")
        .WithOpenApi();
    
    [HttpMethod("/forecast", "POST", RequireAuthorization = false)]
    internal static Results<Ok<WeatherForecast>, ProblemHttpResult> Create(WeatherForecast forecast) =>
       string.IsNullOrEmpty(forecast.Summary)
       ? TypedResults.Problem("Summary cannot be empty or null")
       : TypedResults.Ok(forecast);

}
```

---
## Endpoints with groups  {#grouped-endpoints}
In order to asign endpoints to a group you can use the generic interface of `IEndpoint<TEndpointGroup>` or `IEndpointConfiguration<TEndpointGroup>`  
  
```c#
public sealed class CreateForecast : IEndpoint<WeatherEndpointGroup>
{
    [Post("/forecast", RequireAuthorization = false)]
    internal static Results<Ok<WeatherForecast>, ProblemHttpResult> Create(WeatherForecast forecast) =>
       string.IsNullOrEmpty(forecast.Summary)
       ? TypedResults.Problem("Summary cannot be empty or null")
       : TypedResults.Ok(forecast);
}
```
---
## Attributes {#attributes}

- `Get("/url")`
- `Post("/url")`
- `Put("/url")`
- `Delete("/url")`
- `HttpMethod("/url", "OPTIONS")` this can be used for any other method

#### Configure endpoint authentication using attribute arguments
You can also configure the endpoint's authentication behaviour using the attribute arguments by:    
1. To enable or disable authentication on an endpoint you can use the `bool RequireAuthorization` argument of the attribute.
     - `[Post("/url", RequireAuthorization = true)]` `.RequireAuthorization()` will be added on the `RouteHandlerBuilder` 
     - `[Post("/url", RequireAuthorization = false)]` `.AllowAnonymous()` will be added on the `RouteHandlerBuilder`  

2. Use the `string[]? Policies` argument to allow access only for a specific policy.


>You can also setup Authentication & Authorization on the `Configure(RouteHandlerBuilder builder)` method by implementing the `IEndpointConfiguration` interface.

