using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Primitives;
using System.Threading.RateLimiting;

namespace AspNetRateLimiting;

public class Program
{
    private const string FixedWindowPolicyName = "fixed";
    private const string SlidingWindowPolicyName = "sliding";
    private const string TokenBucketPolicyName = "tokenbucket";
    private const string ConcurrentPolicyName = "concurrent";
    private const string CustomPolicyName = "custom";

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddRateLimiter(options =>
        {
            // ������
            {
                // �̶�����������
                // ʾ��������ÿ����2������
                // ��ʱ�䴰�ڹ��ں������µ�ʱ�䴰�ڣ���������������
                // �㷨���⣺���ڴ��ڱ߽����⣬�������������������ڽ��紦����ô���ܴ�������������������޵�����
                options.AddFixedWindowLimiter(
                    policyName: FixedWindowPolicyName,// �����������Զ���������
                    configureOptions: opt =>
                    {
                        opt.PermitLimit = 2;
                        opt.Window = TimeSpan.FromMinutes(1);
                        opt.QueueLimit = 3;// �����Ŷӵ���������������ֱ�Ӿܾ�����
                        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;// �����Ŷ������˳��OldestFirst��ʾ�ȴ��������Ŷӵ�����
                    });

                // ��������������
                // ʾ��������ÿ����100�����󣬴��ڷ�Ϊ6�μ�ÿ��Ϊ10��
                // ��0�ο�ʼ����6�����ͷ���0�ε����������Դ����ƣ���7�����ͷ���1�ε���������ǰ6�Σ���һ�����ڣ��������������Ʋ�����100��
                // �㷨���⣺�������ڱ߽����⣬������������ƽ�����⣬��һ��ʼ�Ͱ����������꣬��������Ĵ���ʣ��ʱ�����󶼻ᱻ�ܾ�
                options.AddSlidingWindowLimiter(
                    policyName: SlidingWindowPolicyName,
                    configureOptions: options =>
                    {
                        options.PermitLimit = 100;
                        options.Window = TimeSpan.FromMinutes(1);
                        options.SegmentsPerWindow = 6;
                    });

                // ����Ͱ������
                // ʾ�������100�����ƣ����󣩣�ÿ10���Զ�����20������
                options.AddTokenBucketLimiter(
                    policyName: TokenBucketPolicyName,
                    configureOptions: options =>
                    {
                        options.TokenLimit = 100; //���������
                        // true�����������Զ�����ReplenishmentPeriod��ʱ�������ƣ�false����Ҫ�ֶ�����TryReplenish()������
                        // ��asp.net�У�����true����false����������ʱ���ᴥ��������ƣ�ͨ���������ʼ��㵱ǰ�Ŀ������ơ������ǣ�true�ᶨʱ�������������ʱ�������䣬false��ֻ����������ʱ������������ֶ�����TryReplenish()������
                        options.AutoReplenishment = true;
                        options.ReplenishmentPeriod = TimeSpan.FromSeconds(10); // �������Ƶ�ʱ����
                        options.TokensPerPeriod = 20;// ÿ�β�������������������Ƶ��������ʣ�ƽ������ = TokensPerPeriod / ReplenishmentPeriod
                    });

                // ��������������
                // ʾ�����������10����������
                // ��������������ͬ�������������������������������󲢷�����������󲢷����Ƽ�1��������ɺ󲢷����Ƽ�1
                options.AddConcurrencyLimiter(
                    policyName: ConcurrentPolicyName,
                    configureOptions: options =>
                    {
                        options.PermitLimit = 10;
                    });
            }

            // ȫ������������
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        //partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",// ��IP����
                        partitionKey: httpContext.User.Identity?.Name ?? "anonymous",// ���û�����
                        factory: partitionKey => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 10,
                            Window = TimeSpan.FromMinutes(1)
                        })
                );

                // ������ʽ����������˳��ִ�У�
                options.GlobalLimiter = PartitionedRateLimiter.CreateChained(
                    PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    {
                        var userAgent = httpContext.Request.Headers.UserAgent.ToString();

                        return RateLimitPartition.GetFixedWindowLimiter(
                            partitionKey: userAgent,
                            factory: _ =>
                            new FixedWindowRateLimiterOptions
                            {
                                AutoReplenishment = true,
                                PermitLimit = 3,
                                Window = TimeSpan.FromSeconds(2)
                            });
                    }),
                    PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    {
                        var userAgent = httpContext.Request.Headers.UserAgent.ToString();

                        return RateLimitPartition.GetFixedWindowLimiter(
                            partitionKey: userAgent,
                            factory: _ =>
                            new FixedWindowRateLimiterOptions
                            {
                                AutoReplenishment = true,
                                PermitLimit = 30,
                                Window = TimeSpan.FromSeconds(20)
                            });
                    })
                );
            }

            // ���޵Ĵ���ʽ
            {
                // ���������ܾ�����ʱ��״̬��
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                // ���������ܾ�����Ļص�
                options.OnRejected = async (context, cancellationToken) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.HttpContext.Response.Headers["Retry-After"] = "60";
                    await context.HttpContext.Response.WriteAsync("Rate limit exceeded. Please try again later.", cancellationToken);
                };
            }

            // [EnableRateLimiting]��[DisableRateLimiting]���Կ�Ӧ���ڿ���������������

            // �Զ�����������
            options.AddPolicy<string, CustomRateLimiterPolicy>(CustomPolicyName);
        });

        var app = builder.Build();

        // ���������м��
        app.UseRateLimiter();

        // api������ָ��ʹ�õ�������
        app.MapGet("/", () => "Hello World!").RequireRateLimiting(FixedWindowPolicyName);

        app.Run();
    }
}

/// <summary>
/// �Զ�����������
/// </summary>
public class CustomRateLimiterPolicy : IRateLimiterPolicy<string>
{
    public Func<OnRejectedContext, CancellationToken, ValueTask>? OnRejected => (ctx, token) =>
    {
        ctx.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        return ValueTask.CompletedTask;
    };

    public RateLimitPartition<string> GetPartition(HttpContext httpContext)
    {
        // �����û���������
        /*
        {
            var username = "anonymous user";
            if (httpContext.User.Identity?.IsAuthenticated is true && !string.IsNullOrEmpty(httpContext.User.Identity?.Name))
            {
                username = httpContext.User.Identity.Name;
            }

            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: username,
                factory: partitionKey => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 10,
                    Window = TimeSpan.FromMinutes(1)
                });
        }
        */

        // ����jwt��������
        {
            var accessToken = httpContext.Features.Get<IAuthenticateResultFeature>()?
                .AuthenticateResult?.Properties?.GetTokenValue("access_token")?.ToString() ?? string.Empty;

            return RateLimitPartition.GetTokenBucketLimiter(
                partitionKey: StringValues.IsNullOrEmpty(accessToken) ? "Anon" : accessToken,
                factory: _ => new TokenBucketRateLimiterOptions
                {
                    TokenLimit = 100,
                    AutoReplenishment = true,
                    ReplenishmentPeriod = TimeSpan.FromSeconds(10),
                    TokensPerPeriod = 10
                });
        }
    }
}