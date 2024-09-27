using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatabaseDistributedLock;

public class Program
{
    static long s_stock = 0;
    static long s_seed = 0;

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var connStr = "server=localhost;user=root;password=root;database=testing";
        builder.Services.AddDatabaseLocker(options =>
        {
            options.UseMySql(connStr, MySqlServerVersion.AutoDetect(connStr), mysqlOptions =>
            {
                mysqlOptions.EnableRetryOnFailure(3);
            });
        });

        var app = builder.Build();

        app.UseDatabaseLocker();

        app.MapGet("/stock/{stock:long}", SettingStockAsync);
        app.MapGet("/consume", ConsumeAsync);

        app.Run();
    }

    /// <summary>
    /// 初始化库存
    /// </summary>
    /// <param name="stock">库存数量</param>
    /// <returns></returns>
    public static Task<Ok<string>> SettingStockAsync([FromRoute] long stock)
    {
        s_stock = stock;
        return Task.FromResult(TypedResults.Ok($"设置库存数量{stock}"));
    }

    /// <summary>
    /// 秒杀
    /// </summary>
    /// <returns></returns>
    public static async Task<Results<Ok<string>, BadRequest<string>>> ConsumeAsync([FromServices] DatabaseLocker databaseLocker)
    {
        var id = $"{Environment.CurrentManagedThreadId}:{Interlocked.Increment(ref s_seed)}";//需要生成唯一标识，真实的生产环境可使用分布式雪花id

        //获取库存
        if (s_stock < 1) return TypedResults.BadRequest($"秒杀失败，库存不足，id{id}");

        //获取database分布式锁
        using var locker = await databaseLocker.LockAsync("StockLock", id, TimeSpan.FromSeconds(10), true);

        //重新获取库存(因为采用双if+lock锁模式)
        if (s_stock < 1) return TypedResults.BadRequest($"秒杀失败，库存不足，id{id}");

        //扣减库存
        s_stock -= 1;
        var stock = s_stock;

        //释放database分布式锁
        var releaseResult = await locker.ReleaseAsync();

        return TypedResults.Ok($"秒杀成功，id{id}，解锁{(releaseResult.Succeeded ? "成功" : "失败")}，锁{(releaseResult.Released ? "已" : "未")}释放，剩余库存{stock}");
    }
}

