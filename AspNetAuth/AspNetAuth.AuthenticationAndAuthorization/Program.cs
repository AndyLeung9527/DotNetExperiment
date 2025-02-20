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

        // ע��һ�������֤����, ��δָ��Ĭ�������֤����ʱ, ��ʹ�ô˷���
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            // ʹ��cookie��Ϊ�����֤����
            .AddCookie(options =>
            {
                options.LoginPath = "/Account/Login";
            });

        // ע��һ����Ȩ����
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

