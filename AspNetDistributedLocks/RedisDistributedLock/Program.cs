using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace RedisDistributedLock;

public class Program
{
    static long seed = 0;

    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddRedisLocker(options =>
        {
            options.EndPoints.Add("127.0.0.1", 6379);
            options.AbortOnConnectFail = false;
        });

        var app = builder.Build();

        app.MapGet("/stock/{stock:long}", SettingStockAsync);
        app.MapGet("/consume", ConsumeAsync);

        app.Run();
    }

    /// <summary>
    /// 初始化库存
    /// </summary>
    /// <param name="stock">库存数量</param>
    /// <returns></returns>
    public static async Task<Results<Ok<string>, BadRequest<string>>> SettingStockAsync([FromRoute] long stock, [FromServices] IOptions<ConfigurationOptions> options)
    {
        var connectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(options.Value);
        var settingResult = await connectionMultiplexer.GetDatabase(default).StringSetAsync("StockNum", stock);
        if (settingResult)
            return TypedResults.Ok($"设置库存数量{stock}");
        else
            return TypedResults.BadRequest("设置库存失败");
    }

    /// <summary>
    /// 秒杀
    /// </summary>
    /// <returns></returns>
    public static async Task<Results<Ok<string>, BadRequest<string>>> ConsumeAsync([FromServices] RedisLocker redisLocker, [FromServices] IOptions<ConfigurationOptions> options)
    {
        var id = $"{Environment.CurrentManagedThreadId}:{Interlocked.Increment(ref seed)}";//需要生成唯一标识，真实的生产环境可使用分布式雪花id
        var connectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(options.Value);

        //获取库存
        long.TryParse(await connectionMultiplexer.GetDatabase(default).StringGetAsync("StockNum"), out var stockNum);
        if (stockNum < 1) return TypedResults.BadRequest($"秒杀失败，库存不足，id{id}");

        //获取redis分布式锁
        using var locker = await redisLocker.LockAsync("StockLock", id, TimeSpan.FromSeconds(3), true);

        //重新获取库存(因为采用双if+lock锁模式)
        long.TryParse(await connectionMultiplexer.GetDatabase(default).StringGetAsync("StockNum"), out stockNum);
        if (stockNum < 1) return TypedResults.BadRequest($"秒杀失败，库存不足，id{id}");

        //扣减库存
        stockNum -= 1;
        var settingResult = await connectionMultiplexer.GetDatabase(default).StringSetAsync("StockNum", stockNum);
        if (!settingResult) return TypedResults.BadRequest("设置库存失败");

        //释放redis分布式锁
        var releaseResult = await locker.ReleaseAsync();
        return TypedResults.Ok($"秒杀成功，id{id}，解锁{(releaseResult.Succeeded ? "成功" : "失败")}，锁{(releaseResult.Released ? "已" : "未")}释放，剩余库存{stockNum}");
    }
}