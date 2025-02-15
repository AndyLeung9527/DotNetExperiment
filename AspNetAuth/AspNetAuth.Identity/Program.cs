using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using System.Security.Claims;

namespace AspNetAuth.Identity;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddOpenApi();
        builder.Services.AddControllers();

        // Asp.Net框架自带的用户管理服务, Microsoft.AspNetCore.Identity
        // IdentityUser用户类, IdentityRole角色类, 可继承扩展
        // 使用builder.Services.AddIdentityCore则会忽略角色相关
        // 方法内执行了AddAuthentication().AddCookie(), 默认身份验证方案名IdentityConstants.ApplicationScheme, 登录页/Account/Login(可通过builder.Services.ConfigureApplicationCookie修改), 并注入了UserManager、RoleManager等服务
        builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
        {
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireDigit = false;
            options.SignIn.RequireConfirmedEmail = false;// 是否需要确认邮件才允许登录
            options.Tokens.EmailConfirmationTokenProvider = "EmailConfirmation";// 使用自定义的邮件令牌提供器
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);// 锁定时长
            options.Lockout.MaxFailedAccessAttempts = 3;// 最大尝试次数
        })
        // 使用EFCore并指定存储用户、角色等信息的DbContext(继承自IdentityDbContext)
        .AddEntityFrameworkStores<ApplicationDbContext>()
        // 令牌生成器, 用于生成重置密码令牌、电子邮件确认令牌等,
        .AddDefaultTokenProviders()
        // 增加自定义的邮件令牌提供器
        .AddTokenProvider<EmailConfirmationTokenProvider<IdentityUser>>("EmailConfirmation")
        // 增加自定义密码验证器
        .AddPasswordValidator<CustomPasswordValidator<IdentityUser>>();

        // 通过修改DataProtectionTokenProviderOptions配置设置令牌有效期等
        builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
        {
            options.TokenLifespan = TimeSpan.FromHours(2);
        });

        // 单独设置邮件令牌的有效期(通过自定义的邮件令牌提供器), 否则沿用上面的默认配置
        builder.Services.Configure<EmailConfirmationTokenProviderOptions>(options =>
        {
            options.TokenLifespan = TimeSpan.FromDays(2);
        });

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
        });

        var app = builder.Build();

        app.MapOpenApi();
        app.MapScalarApiReference();
        app.MapControllers();

        app.MapGet("/", async context =>
        {
            var userName = context.User.Claims.FirstOrDefault(o => ClaimTypes.Name.Equals(o.Type))?.Value;
            context.Response.ContentType = "text/plain; charset=utf-8";
            await context.Response.WriteAsync($"用户：{userName}已登录");
        }).RequireAuthorization();

        app.Run();
    }
}
