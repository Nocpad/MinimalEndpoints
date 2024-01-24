using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nocpad.AspNetCore.MinimalEndpoints;
using System;
using System.Linq;
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
    [Get("tests sa ")]
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


[Endpoint<WeatherGroup>]
internal sealed class GetWeatherX : IEndpointConfiguration
{
    public static void Configure(RouteHandlerBuilder builder)
    {
        throw new NotImplementedException();
    }

    [Get("tests sa ")]
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
