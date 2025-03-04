using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Registry;
using Polly.Retry;

namespace DotNetResilience;

internal class Program
{
    static async Task Main(string[] args)
    {
        // 基本用法
        {
            var pipeline = new ResiliencePipelineBuilder()
                .AddRetry(new RetryStrategyOptions
                {
                    ShouldHandle = new PredicateBuilder().Handle<Exception>(),// 处理的异常类型
                    Delay = TimeSpan.FromSeconds(2),// 重试间隔时间
                    MaxRetryAttempts = 2,// 最大重试次数
                    BackoffType = DelayBackoffType.Constant,// 重试间隔时间的变化类型
                    UseJitter = true// 是否使用随机化的间隔时间
                })
                .AddTimeout(TimeSpan.FromSeconds(10))// 超时时间
                .Build();

            int executionCount = 1;
            await pipeline.ExecuteAsync(async ct =>
            {
                Console.WriteLine($"第{executionCount++}次执行");
                throw new Exception();
            });
        }

        // DI
        {
            var services = new ServiceCollection();
            services.AddResiliencePipeline("default", builder =>
            {
                builder.AddRetry(new RetryStrategyOptions
                {
                    ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                    Delay = TimeSpan.FromSeconds(2),
                    MaxRetryAttempts = 2,
                    BackoffType = DelayBackoffType.Constant,
                    UseJitter = true
                });
                builder.AddTimeout(TimeSpan.FromSeconds(10));
            });
            var serviceProvider = services.BuildServiceProvider();
            var pipelineProvider = serviceProvider.GetRequiredService<ResiliencePipelineProvider<string>>();
            var pipeline = pipelineProvider.GetPipeline("default");

            int executionCount = 1;
            await pipeline.ExecuteAsync(async ct =>
            {
                Console.WriteLine($"第{executionCount++}次执行");
                throw new Exception();
            });

            // 使用HttpClient时可直接使用官方标准的Resilience管道, 需要安装nuget包Microsoft.Extensions.Http.Resilience
            // services.AddHttpClient<T>().AddStandardResiliencePipelineHandler();
        }

        Console.WriteLine("执行完成");
        Console.ReadLine();
    }
}
