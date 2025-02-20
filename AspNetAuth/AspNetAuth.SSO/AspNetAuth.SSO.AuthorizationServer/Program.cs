using Scalar.AspNetCore;

namespace AspNetAuth.SSO.AuthorizationServer;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddOpenApi();
        builder.Services.AddControllers();

        builder.Services.Configure<JwtTokenOptions>(builder.Configuration.GetSection("JwtToken"));

        var app = builder.Build();

        app.MapOpenApi();
        app.MapScalarApiReference();
        app.MapControllers();

        app.Run();
    }
}

