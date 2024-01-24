using Microsoft.AspNetCore.Builder;
using Nocpad.AspNetCore.MinimalEndpoints;
using System;

namespace TestProject;

[Endpoint(Active = false)]

internal sealed class WeatherGroup : IEndpointGroup
{
    public static string Name => "weather";
    public static string Route => "api/weather";
}


[Endpoint<WeatherGroup>]
internal sealed class WeatherGroup2
{
    public static string Name => "weather";
    public static string Route => "api/weather";
}
