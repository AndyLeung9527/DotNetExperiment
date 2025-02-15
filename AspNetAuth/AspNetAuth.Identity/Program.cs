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

        // Asp.Net����Դ����û��������, Microsoft.AspNetCore.Identity
        // IdentityUser�û���, IdentityRole��ɫ��, �ɼ̳���չ
        // ʹ��builder.Services.AddIdentityCore�����Խ�ɫ���
        // ������ִ����AddAuthentication().AddCookie(), Ĭ�������֤������IdentityConstants.ApplicationScheme, ��¼ҳ/Account/Login(��ͨ��builder.Services.ConfigureApplicationCookie�޸�), ��ע����UserManager��RoleManager�ȷ���
        builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
        {
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireDigit = false;
            options.SignIn.RequireConfirmedEmail = false;// �Ƿ���Ҫȷ���ʼ��������¼
            options.Tokens.EmailConfirmationTokenProvider = "EmailConfirmation";// ʹ���Զ�����ʼ������ṩ��
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);// ����ʱ��
            options.Lockout.MaxFailedAccessAttempts = 3;// ����Դ���
        })
        // ʹ��EFCore��ָ���洢�û�����ɫ����Ϣ��DbContext(�̳���IdentityDbContext)
        .AddEntityFrameworkStores<ApplicationDbContext>()
        // ����������, �������������������ơ������ʼ�ȷ�����Ƶ�,
        .AddDefaultTokenProviders()
        // �����Զ�����ʼ������ṩ��
        .AddTokenProvider<EmailConfirmationTokenProvider<IdentityUser>>("EmailConfirmation")
        // �����Զ���������֤��
        .AddPasswordValidator<CustomPasswordValidator<IdentityUser>>();

        // ͨ���޸�DataProtectionTokenProviderOptions��������������Ч�ڵ�
        builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
        {
            options.TokenLifespan = TimeSpan.FromHours(2);
        });

        // ���������ʼ����Ƶ���Ч��(ͨ���Զ�����ʼ������ṩ��), �������������Ĭ������
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
            await context.Response.WriteAsync($"�û���{userName}�ѵ�¼");
        }).RequireAuthorization();

        app.Run();
    }
}
