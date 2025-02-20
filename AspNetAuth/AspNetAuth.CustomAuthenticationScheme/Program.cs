using Scalar.AspNetCore;

namespace AspNetAuth.CustomAuthenticationScheme;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddOpenApi();
        builder.Services.AddControllers();

        // ����Զ���������֤����, ������ΪĬ�ϵ������֤����
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
