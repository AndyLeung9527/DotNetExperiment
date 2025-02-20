using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Scalar.AspNetCore;

namespace AspNetAuth.CustomAuthorizationPolicy;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddOpenApi();
        builder.Services.AddControllers();

        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.LoginPath = "/Account/Login";
            });

        // 注册自定义授权策略
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("CustomPolicy", policyBuilder =>
            {
                policyBuilder.Requirements.Add(new CustomAuthorizationRequirement());
            });
        });
        builder.Services.AddSingleton<IAuthorizationHandler, Mail163Handler>();
        builder.Services.AddSingleton<IAuthorizationHandler, MailQqHandler>();

        var app = builder.Build();

        app.MapOpenApi();
        app.MapScalarApiReference();
        app.MapControllers();

        app.Run();
    }
}
