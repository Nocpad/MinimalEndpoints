using Nocpad.AspNetCore.MinimalEndpoints;

namespace TestProject;
internal sealed class WeatherGroup : IEndpointGroup
{
    public static string Name => "weather";
    public static string Route => "api/weather";
}