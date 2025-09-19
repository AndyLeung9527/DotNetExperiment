using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

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

        // 身份验证方案3（自定义身份验证）
        builder.Services.AddAuthentication().AddScheme<CustomAuthenticationSchemeOptions, CustomAuthenticationHandler>(CustomAuthenticationHandler.SchemeName, builder.Configuration.GetRequiredSection("Authentication").Bind);

        var app = builder.Build();

        app.MapOpenApi();
        app.MapScalarApiReference();
        app.MapControllers();

        app.Run();
    }
}

file class CustomAuthenticationHandler : AuthenticationHandler<CustomAuthenticationSchemeOptions>
{
    public const string SchemeName = "CustomSchemeName";

    private const string HeaderName = "Authorization";
    private const string CredentialPrefix = "Basic ";

    public CustomAuthenticationHandler(IOptionsMonitor<CustomAuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (Request.Headers.TryGetValue(HeaderName, out var value) && !string.IsNullOrWhiteSpace(value))
        {
            var authorization = value.ToString();
            if (authorization.StartsWith(CredentialPrefix))
            {
                var token = authorization.Substring(CredentialPrefix.Length);
                var credential = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Options.UserName}:{Options.Password}"));
                if (token.Equals(credential))
                {
                    var claims = new[] { new Claim(ClaimTypes.Name, "UserName") };
                    var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, SchemeName));
                    var ticket = new AuthenticationTicket(principal, SchemeName);

                    return Task.FromResult(AuthenticateResult.Success(ticket));
                }
            }
        }

        return Task.FromResult(AuthenticateResult.Fail("Missing Authorization"));
    }
}

file class CustomAuthenticationSchemeOptions : AuthenticationSchemeOptions
{
    public string UserName { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}