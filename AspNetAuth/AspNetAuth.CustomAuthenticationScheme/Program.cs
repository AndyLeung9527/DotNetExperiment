using Scalar.AspNetCore;

namespace AspNetAuth.CustomAuthenticationScheme;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddOpenApi();
        builder.Services.AddControllers();

        // 添加自定义的身份验证方案, 并设置为默认的身份验证方案
        builder.Services.AddAuthentication(options =>
        {
            options.AddScheme<CustomAuthenticationHandler>(CustomAuthenticationDefaults.AuthenticationScheme, "custom-demo");
            options.DefaultAuthenticateScheme = CustomAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = CustomAuthenticationDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = CustomAuthenticationDefaults.AuthenticationScheme;
            options.DefaultForbidScheme = CustomAuthenticationDefaults.AuthenticationScheme;
            options.DefaultSignOutScheme = CustomAuthenticationDefaults.AuthenticationScheme;
        });

        var app = builder.Build();

        app.MapOpenApi();
        app.MapScalarApiReference();
        app.MapControllers();

        app.Run();
    }
}
