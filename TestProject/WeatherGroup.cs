namespace TestProject;

[Nocpad.AspNetCore.MinimalEndpoints.Endpoint(Active = false)]

internal sealed class WeatherGroup
{
    public static string Name => "weather";
    public static string Route => "api/weather";
}