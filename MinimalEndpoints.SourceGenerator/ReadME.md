# MinimalEndpoints

A source generator for ASP.NET Core Minimal APIs that removes the repetitive boilerplate needed to register endpoint handlers.

It uses attributes and marker interfaces to discover endpoints at compile time and generates the `MapMinimalEndpoints()` and `AddMinimalEndpoints()` registration code for you.

## Features

- Automatically maps classes marked with `[Endpoint]`
- Supports grouped endpoints via `[Endpoint<TGroup>]`
- Supports per-endpoint configuration via `IEndpointConfiguration`
- Supports group configuration via `IEndpointGroupConfiguration`
- Supports HTTP verb attributes: `[Get]`, `[Post]`, `[Put]`, `[Delete]`, `[Patch]`, `[Head]`, `[Query]`
- Supports optional authorization metadata (`RequireAuthorization`, `Policies`)
- Supports optional FluentValidation integration with `[Validate<T>]`

## Installation

```bash
dotnet add package Nocpad.AspNetCore.MinimalEndpoints
```

## Quick start

```csharp
using Nocpad.AspNetCore.MinimalEndpoints;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapMinimalEndpoints();

app.Run();
```

```csharp
using Nocpad.AspNetCore.MinimalEndpoints;

[Endpoint]
internal sealed class Upload
{
    [Post("/file-upload")]
    public static IResult Handle(IFormFile file) => Results.Ok(new
    {
        file.FileName,
        file.ContentType,
        file.Length
    });
}
```

## Endpoint configuration

If an endpoint class implements `IEndpointConfiguration`, the generator calls `Configure(RouteHandlerBuilder builder)` for each mapped endpoint.

```csharp
using Nocpad.AspNetCore.MinimalEndpoints;

[Endpoint]
internal sealed class Upload : IEndpointConfiguration
{
    public static void Configure(RouteHandlerBuilder builder)
    {
        builder.ProducesValidationProblem();
        builder.WithName("UploadFile");
    }

    [Post("/file-upload")]
    public static IResult Handle(IFormFile file) => Results.Ok(new
    {
        file.FileName,
        file.ContentType,
        file.Length
    });
}
```

## Endpoint groups

Define a route group with a static `Route`, `Name`, and optional `Configure(RouteGroupBuilder group)` method.

```csharp
using Nocpad.AspNetCore.MinimalEndpoints;

internal sealed class WeatherEndpointGroup : IEndpointGroupConfiguration
{
    public static string Route => "api/weather";

    public static string Name => "Weather";

    public static void Configure(RouteGroupBuilder group) => group.RequireAuthorization();
}

[Endpoint<WeatherEndpointGroup>]
internal sealed class GetWeather
{
    [Get("forecast")]
    internal static WeatherForecast[] Get()
    {
        return [];
    }
}
```

This generates a `MapGroup("api/weather")` registration and applies the configured group metadata.

## Authorization and validation

You can assign authorization and validation attributes directly on the HTTP method.

```csharp
using Nocpad.AspNetCore.MinimalEndpoints;

[Endpoint]
internal sealed class UserEndpoints
{
    [Validate<CreateUserRequest>, Post("users", Policies = ["Admin"], RequireAuthorization = true)]
    public static IResult Create(CreateUserRequest request) => Results.Ok();
}
```

When FluentValidation is referenced by the project, the generated validation filter is available automatically.

## Dependency registration

The generator also adds an `AddMinimalEndpoints()` extension that can be used to register endpoint classes as services when required.

```csharp
builder.Services.AddMinimalEndpoints();
```

## Notes

- The source generator scans classes marked with `[Endpoint]` or `[Endpoint<TGroup>]`.
- Each method decorated with an HTTP verb attribute becomes a route handler.
- The generated code is created during compilation, so no manual endpoint mapping code is required in `Program.cs` beyond `app.MapMinimalEndpoints();`.
