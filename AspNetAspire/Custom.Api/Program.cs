using Microsoft.AspNetCore.Mvc;

namespace Custom.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        app.MapGet("/", () => "Hello World!");

        app.MapPost("/httpCommand", static async (
            [FromHeader(Name = "X-Invalidation-Key")] string? header
            ) =>
        {
            bool valid = false;
            if (header is not null) valid = true;

            await Task.CompletedTask;

            if (!valid) return Results.Unauthorized();
            else return Results.Ok();
        });

        app.Run();
    }
}
