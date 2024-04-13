using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nocpad.AspNetCore.MinimalEndpoints;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using TestProject;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapMinimalEndpoints();

app.Run();


[Endpoint]
internal sealed class GetWeather
{
    [Validate<GetWeather>, Get("tests sa ")]
    internal static WeatherForecast[] Get()
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
       new WeatherForecast
       (
           DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
           Random.Shared.Next(-20, 55),
           WeatherForecast.Summaries[Random.Shared.Next(WeatherForecast.Summaries.Length)]
       ))
       .ToArray();
        return forecast;
    }
}
//app.MapMethods("tests sa ", new[] { "GET" }, global::GetWeatherX1.Get).RequireAuthorization("a", "s")

[Endpoint<WeatherGroup2>]
internal sealed class GetWeatherX : IEndpointConfiguration
{
    public static void Configure(RouteHandlerBuilder builder)
    {
        throw new NotImplementedException();
    }

    [Validate<GetWeatherX>, Get("", Policies = ["1", "22dsad"], RequireAuthorization = true)]
    internal static WeatherForecast[] Get()
    {
        var forecast = Enumerable.Range(1, 5)
            .Select(index => new WeatherForecast
            (
                DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                Random.Shared.Next(-20, 55),
                WeatherForecast.Summaries[Random.Shared.Next(WeatherForecast.Summaries.Length)]
            ))
            .ToArray();
        return forecast;
    }
}

[Endpoint<WeatherGroup>]
internal sealed class GetWeatherX1 : IEndpointConfiguration
{
    public static void Configure(RouteHandlerBuilder builder) { }

    [Get("weather-get")]
    internal static WeatherForecast[] Get()
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
       new WeatherForecast
       (
           DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
           Random.Shared.Next(-20, 55),
           WeatherForecast.Summaries[Random.Shared.Next(WeatherForecast.Summaries.Length)]
       ))
       .ToArray();
        return forecast;
    }
}



internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    internal static string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}


//[Endpoint]
//internal sealed class Upload : IEndpointConfiguration
//{
//    public static void Configure(RouteHandlerBuilder builder)
//    {
//        throw new NotImplementedException();
//    }

//    [Post("/file-upload")]
//    public static IResult Handle(IFormFile file) => Results.Ok(new { file.FileName, file.ContentType, file.Length });
//}