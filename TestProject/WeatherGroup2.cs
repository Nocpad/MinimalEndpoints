using Nocpad.AspNetCore.MinimalEndpoints;

namespace TestProject;

internal sealed class WeatherGroup2 : IEndpointGroup
{
    public static string Name => "Weather 2";

    public static string Route => "api/weather2";
}
