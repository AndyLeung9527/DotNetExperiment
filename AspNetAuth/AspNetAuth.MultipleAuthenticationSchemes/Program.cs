using Microsoft.AspNetCore.Authentication.Cookies;
using Scalar.AspNetCore;

namespace AspNetAuth.MultipleAuthenticationSchemes;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddOpenApi();
        builder.Services.AddControllers();

        // 身份验证方案1
        builder.Services.AddAuthentication($"{CookieAuthenticationDefaults.AuthenticationScheme}_1")
            .AddCookie($"{CookieAuthenticationDefaults.AuthenticationScheme}_1", options =>
            {
                options.Cookie.Name = $"{CookieAuthenticationDefaults.AuthenticationScheme}_1";
                options.LoginPath = "/Account/Login_1";
            });

        // 身份验证方案2
        builder.Services.AddAuthentication($"{CookieAuthenticationDefaults.AuthenticationScheme}_2")
            .AddCookie($"{CookieAuthenticationDefaults.AuthenticationScheme}_2", options =>
            {
                options.Cookie.Name = $"{CookieAuthenticationDefaults.AuthenticationScheme}_2";
                options.LoginPath = "/Account/Login_2";
            });

        var app = builder.Build();

        app.MapOpenApi();
        app.MapScalarApiReference();
        app.MapControllers();

        app.Run();
    }
}
