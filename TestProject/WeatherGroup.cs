using Nocpad.AspNetCore.MinimalEndpoints;

namespace TestProject;


internal sealed class WeatherGroup : IEndpointGroup
{
    public static string Name => "WWeather";
    public static string Route => "api/weather";
}


internal sealed class WeatherGroup2
{
    public static string Name => "Weather";
    public static string Route => "api/weather";
}
