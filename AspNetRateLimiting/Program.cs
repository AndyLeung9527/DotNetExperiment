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
            // 限流器
            {
                // 固定窗口限流器
                // 示例中允许每分钟2个请求
                // 当时间窗口过期后，启动新的时间窗口，并重置请求限制
                // 算法问题：存在窗口边界问题，当流量集中在两个窗口交界处，那么可能存在最大流量是设置上限的两倍
                options.AddFixedWindowLimiter(
                    policyName: FixedWindowPolicyName,// 根据命名策略定义限流器
                    configureOptions: opt =>
                    {
                        opt.PermitLimit = 2;
                        opt.Window = TimeSpan.FromMinutes(1);
                        opt.QueueLimit = 3;// 允许排队的请求数，超过则直接拒绝请求
                        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;// 处理排队请求的顺序，OldestFirst表示先处理最早排队的请求
                    });

                // 滑动窗口限流器
                // 示例中允许每分钟100个请求，窗口分为6段即每段为10秒
                // 从0段开始，到6段则释放在0段的请求数，以此类推，到7段则释放在1段的请求数，前6段（即一分钟内）的总请求数限制不超过100个
                // 算法问题：虽解决窗口边界问题，存在限流不够平滑问题，当一开始就把请求数用完，则接下来的窗口剩余时间请求都会被拒绝
                options.AddSlidingWindowLimiter(
                    policyName: SlidingWindowPolicyName,
                    configureOptions: options =>
                    {
                        options.PermitLimit = 100;
                        options.Window = TimeSpan.FromMinutes(1);
                        options.SegmentsPerWindow = 6;
                    });

                // 令牌桶限流器
                // 示例中最多100个令牌（请求），每10秒自动补充20个令牌
                options.AddTokenBucketLimiter(
                    policyName: TokenBucketPolicyName,
                    configureOptions: options =>
                    {
                        options.TokenLimit = 100; //最大令牌数
                        // true：限流器会自动按照ReplenishmentPeriod定时补充令牌，false：需要手动调用TryReplenish()来补充
                        // 在asp.net中，无论true或者false，当请求到来时都会触发补充机制，通过补充速率计算当前的可用令牌。区别是，true会定时补充或者请求到来时触发补充，false则只会在请求到来时触发补充或者手动调用TryReplenish()来补充
                        options.AutoReplenishment = true;
                        options.ReplenishmentPeriod = TimeSpan.FromSeconds(10); // 补充令牌的时间间隔
                        options.TokensPerPeriod = 20;// 每次补充的令牌数，控制令牌的生成速率，平均速率 = TokensPerPeriod / ReplenishmentPeriod
                    });

                // 并发请求限流器
                // 示例中允许最多10个并发请求
                // 与上述限流器不同，并不限制请求总数，而是限制请求并发数，请求发起后并发限制减1，请求完成后并发限制加1
                options.AddConcurrencyLimiter(
                    policyName: ConcurrentPolicyName,
                    configureOptions: options =>
                    {
                        options.PermitLimit = 10;
                    });
            }

            // 全局限流并分区
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        //partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",// 按IP分区
                        partitionKey: httpContext.User.Identity?.Name ?? "anonymous",// 按用户分区
                        factory: partitionKey => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 10,
                            Window = TimeSpan.FromMinutes(1)
                        })
                );

                // 创建链式限流器（按顺序执行）
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

            // 受限的处理方式
            {
                // 设置限流拒绝请求时的状态码
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                // 设置限流拒绝请求的回调
                options.OnRejected = async (context, cancellationToken) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.HttpContext.Response.Headers["Retry-After"] = "60";
                    await context.HttpContext.Response.WriteAsync("Rate limit exceeded. Please try again later.", cancellationToken);
                };
            }

            // [EnableRateLimiting]和[DisableRateLimiting]特性可应用于控制器、操作方法

            // 自定义限流策略
            options.AddPolicy<string, CustomRateLimiterPolicy>(CustomPolicyName);
        });

        var app = builder.Build();

        // 启用限流中间件
        app.UseRateLimiter();

        // api限流，指定使用的限流器
        app.MapGet("/", () => "Hello World!").RequireRateLimiting(FixedWindowPolicyName);

        app.Run();
    }
}

/// <summary>
/// 自定义限流策略
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
        // 根据用户分区限流
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

        // 根据jwt分区限流
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