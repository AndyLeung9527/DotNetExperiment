using Microsoft.AspNetCore.Authentication.Cookies;
using Scalar.AspNetCore;
using System.Security.Claims;

namespace AspNetAuth.AuthenticationAndAuthorization;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddOpenApi();
        builder.Services.AddControllers();

        // 注册一个身份验证方案, 当未指定默认身份验证方案时, 将使用此方案
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            // 使用cookie作为身份验证方案
            .AddCookie(options =>
            {
                options.LoginPath = "/Account/Login";
            });

        // 注册一个授权策略
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("CustomPolicy", policyBuilder =>
            {
                policyBuilder.RequireRole("Admin", "Guest").RequireClaim(ClaimTypes.Email).RequireAssertion(context =>
                {
                    return context.User.Claims.FirstOrDefault(o => o.Type.Equals(ClaimTypes.Email))?.Value.EndsWith("@163.com") ?? false;
                });
            });
        });

        var app = builder.Build();

        app.MapOpenApi();
        app.MapScalarApiReference();
        app.MapControllers();

        app.Run();
    }
}

