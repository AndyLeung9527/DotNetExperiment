using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Security.Cryptography;

namespace AspNetAuth.SSO.WebApi1;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddOpenApi();
        builder.Services.AddControllers();

        var jwtTokenOptions = new JwtTokenOptions();
        builder.Configuration.Bind("JwtToken", jwtTokenOptions);

        // 注册jwt验证, 请求头中需要带上Authorization: Bearer {token}

        // 对称加密
        {
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,// 是否验证Issuer
                        ValidateAudience = true,// 是否验证Audience
                        ValidateLifetime = true,// 是否验证失效时间
                        ClockSkew = TimeSpan.Zero,// 过期后立即失效
                        ValidateIssuerSigningKey = true,// 是否验证SecurityKey
                        ValidIssuer = jwtTokenOptions.Issuer,
                        ValidAudience = jwtTokenOptions.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtTokenOptions.SecurityKey))
                    };
                    //options.Events = new JwtBearerEvents()
                    //{
                    //    // 身份验证不通过处理
                    //    OnChallenge = context =>
                    //    {
                    //    }
                    //};
                });
        }

        // 非对称加密
        //{
        //    var currentDir = Directory.GetCurrentDirectory();
        //    var parentDir = Directory.GetParent(currentDir)!.FullName;
        //    var publicKeyPem = File.ReadAllText(Path.Combine(parentDir, "AspNetAuth.SSO.AuthorizationServer", "public.pem"));
        //    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        //        .AddJwtBearer(options =>
        //        {
        //            options.TokenValidationParameters = new TokenValidationParameters
        //            {
        //                ValidateIssuer = true,// 是否验证Issuer
        //                ValidateAudience = true,// 是否验证Audience
        //                ValidateLifetime = true,// 是否验证失效时间
        //                ClockSkew = TimeSpan.Zero,// 过期后立即失效
        //                ValidateIssuerSigningKey = true,// 是否验证SecurityKey
        //                ValidIssuer = jwtTokenOptions.Issuer,
        //                ValidAudience = jwtTokenOptions.Audience,
        //                IssuerSigningKey = new RsaSecurityKey(RsaKeyGenerator.ConvertStringToRSAParameters(publicKeyPem))
        //            };
        //        });
        //}

        var app = builder.Build();

        app.MapOpenApi();
        app.MapScalarApiReference();
        app.MapControllers();

        app.Run();
    }
}

public static class RsaKeyGenerator
{
    public static RSAParameters ConvertStringToRSAParameters(string publicKeyPem)
    {
        using var rsa = RSA.Create();
        rsa.ImportFromPem(publicKeyPem);
        return rsa.ExportParameters(false);
    }
}