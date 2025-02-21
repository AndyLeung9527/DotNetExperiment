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

        // ע��jwt��֤, ����ͷ����Ҫ����Authorization: Bearer {token}

        // �ԳƼ���
        {
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,// �Ƿ���֤Issuer
                        ValidateAudience = true,// �Ƿ���֤Audience
                        ValidateLifetime = true,// �Ƿ���֤ʧЧʱ��
                        ClockSkew = TimeSpan.Zero,// ���ں�����ʧЧ
                        ValidateIssuerSigningKey = true,// �Ƿ���֤SecurityKey
                        ValidIssuer = jwtTokenOptions.Issuer,
                        ValidAudience = jwtTokenOptions.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtTokenOptions.SecurityKey))
                    };

                    // �Զ�����Ӧ
                    //options.Events = new JwtBearerEvents()
                    //{
                    //    // Token����ʧ�ܣ�����ڡ�ǩ������
                    //    OnAuthenticationFailed = context =>
                    //    {
                    //        context.Response.StatusCode = 200;
                    //        context.Response.ContentType = "application/json";
                    //        if (context.Exception is SecurityTokenExpiredException)
                    //        {
                    //            return context.Response.WriteAsJsonAsync(new { code = 401, message = "Token�ѹ���" });
                    //        }
                    //        if (context.Exception is SecurityTokenInvalidSignatureException)
                    //        {
                    //            return context.Response.WriteAsJsonAsync(new { code = 401, message = "Tokenǩ����Ч" });
                    //        }

                    //        return context.Response.WriteAsJsonAsync(new { code = 401, message = "Token��֤ʧ��" });
                    //    },
                    //    // ����δЯ��Token��Token��Ч
                    //    OnChallenge = context =>
                    //    {
                    //        context.HandleResponse();
                    //        if (!context.Response.HasStarted)
                    //        {
                    //            context.Response.StatusCode = 200;
                    //            context.Response.ContentType = "application/json";
                    //            return context.Response.WriteAsJsonAsync(new { code = 401, message = "���¼" });
                    //        }

                    //        return Task.CompletedTask;
                    //    },
                    //    // Token��Ч��Ȩ�޲���
                    //    OnForbidden = context =>
                    //    {
                    //        context.Response.StatusCode = 200;
                    //        context.Response.ContentType = "application/json";
                    //        return context.Response.WriteAsJsonAsync(new { code = 403, message = "��Ȩ�޷���" });
                    //    }
                    //};
                });
        }

        // �ǶԳƼ���
        //{
        //    var currentDir = Directory.GetCurrentDirectory();
        //    var parentDir = Directory.GetParent(currentDir)!.FullName;
        //    var publicKeyPem = File.ReadAllText(Path.Combine(parentDir, "AspNetAuth.SSO.AuthorizationServer", "public.pem"));
        //    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        //        .AddJwtBearer(options =>
        //        {
        //            options.TokenValidationParameters = new TokenValidationParameters
        //            {
        //                ValidateIssuer = true,// �Ƿ���֤Issuer
        //                ValidateAudience = true,// �Ƿ���֤Audience
        //                ValidateLifetime = true,// �Ƿ���֤ʧЧʱ��
        //                ClockSkew = TimeSpan.Zero,// ���ں�����ʧЧ
        //                ValidateIssuerSigningKey = true,// �Ƿ���֤SecurityKey
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