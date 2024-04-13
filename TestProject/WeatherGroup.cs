using Microsoft.AspNetCore.Routing;
using Nocpad.AspNetCore.MinimalEndpoints;

namespace TestProject;


internal sealed class WeatherGroup : IEndpointGroupConfiguration
{
    public static string Name => "Weather";
    public static string Route => "api/weather";

    public static void Configure(RouteGroupBuilder group)
    {

    }
}
