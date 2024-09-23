namespace RedisDistributedLock;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

public class RedisLocker : IDisposable
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly string _acquireScript;

    public RedisLocker(IOptions<ConfigurationOptions> options)
    {
        _connectionMultiplexer = ConnectionMultiplexer.Connect(options.Value);
        _acquireScript = """
            -- 若锁不存在，则新增锁并设置：1.唯一标识，2.重入计数(初始值为1)，3.锁过期时间
            if (redis.call('exists', @key) == 0) then
                redis.call('hset', @key, @id, 1);
                redis.call('pexpire', @key, @expire);
                return nil;
            end;
            -- 若锁存在且唯一标识匹配，设置：1.重入计数+1，2.锁过期时间
            if (redis.call('hexists', @key, @id) == 1) then
                redis.call('hincrby', @key, @id, 1);
                redis.call('pexpire', @key, @expire);
                return nil;
            end;
            -- 若锁存在但唯一标识不匹配，即获取锁失败，返回锁剩余过期时间
            return redis.call('pttl', @key);
            -- 当且仅当返回nil，才表示获取锁成功
            """;
    }

    /// <summary>
    /// 获取锁
    /// </summary>
    /// <param name="key">锁名</param>
    /// <param name="id">唯一标识</param>
    /// <param name="expire">过期时长</param>
    /// <param name="autoRenewal">自动续约</param>
    /// <returns></returns>
    public async Task<Locker> LockAsync(string key, string id, TimeSpan expire, bool autoRenewal = false)
    {
        var cts = new CancellationTokenSource();
        while (true)
        {
            var acquireResult = await _connectionMultiplexer.GetDatabase(default).ScriptEvaluateAsync(LuaScript.Prepare(_acquireScript), new
            {
                key,
                id,
                expire = Convert.ToInt64(expire.TotalMilliseconds),
            });
            if (acquireResult.IsNull)
            {
                if (autoRenewal)
                {
                    Task.Run(async () =>
                    {
                        var delay = expire / 2;
                        await Task.Delay(delay);
                        while (!cts.Token.IsCancellationRequested)
                        {
                            var renewalScript = LuaScript.Prepare("""
                                -- 若锁存在则重新设置过期时间
                                if (redis.call('hexists', @key, @id) == 1) then
                                    redis.call('pexpire', @key, @expire);
                                    return 1;
                                end;
                                return 0;
                                """);
                            var renewalResult = await _connectionMultiplexer.GetDatabase(default).ScriptEvaluateAsync(renewalScript, new
                            {
                                key,
                                id,
                                expire = Convert.ToInt64(expire.TotalMilliseconds),
                            });
                            if (!renewalResult.IsNull && (int)renewalResult == 1) await Task.Delay(delay);
                            else break;
                        }
                    }, cts.Token);
                }
                return new Locker(key, id, expire, _connectionMultiplexer, cts);
            }
        }
    }

    public void Dispose()
    {
        _connectionMultiplexer.Dispose();
    }
}

public class Locker : IDisposable
{
    public string Key { get; }

    public string Id { get; }

    public TimeSpan Expire { get; }

    private readonly IConnectionMultiplexer _connectionMultiplexer;

    private readonly CancellationTokenSource _cts;

    private static string RELEASE_SCRIPT = """
            -- 若锁不存在或唯一标识不匹配则返回nil
            if (redis.call('hexists', @key, @id) == 0) then
                return nil;
            end;
            -- 否则重入计数-1
            local counter = redis.call('hincrby', @key, @id, -1);
            if (counter > 0) then
                redis.call('pexpire', @key, @expire);
                return 0;
            else
                -- 重入计数为0则删除锁
                redis.call('del', @key);
                return 1;
            end;
            return nil;
            """;

    public Locker(string key, string id, TimeSpan expire, IConnectionMultiplexer connectionMultiplexer, CancellationTokenSource cts)
    {
        Key = key;
        Id = id;
        Expire = expire;
        _connectionMultiplexer = connectionMultiplexer;
        _cts = cts;
    }

    /// <summary>
    /// 释放锁
    /// </summary>
    /// <returns>item1: 是否成功, item2: 锁是否被完全释放</returns>
    /// <exception cref="ObjectDisposedException"></exception>
    public async Task<(bool Succeeded, bool Released)> ReleaseAsync()
    {
        if (!_cts.IsCancellationRequested)
        {
            var releaseResult = await _connectionMultiplexer.GetDatabase(default).ScriptEvaluateAsync(LuaScript.Prepare(RELEASE_SCRIPT), new
            {
                key = Key,
                id = Id,
                expire = Convert.ToInt64(Expire.TotalMilliseconds)
            });
            await _cts.CancelAsync();
            _cts.Dispose();
            return (!releaseResult.IsNull, releaseResult.IsNull ? false : (int)releaseResult == 1);
        }
        throw new ObjectDisposedException("重复释放");
    }

    public void Dispose()
    {
        if (!_cts.IsCancellationRequested)
        {
            _connectionMultiplexer.GetDatabase(default).ScriptEvaluate(LuaScript.Prepare(RELEASE_SCRIPT), new
            {
                key = Key,
                id = Id,
                expire = Convert.ToInt64(Expire.TotalMilliseconds)
            });
            _cts.Cancel();
            _cts.Dispose();
        }
    }
}

public static class RedisLockerExtensions
{
    public static void AddRedisLocker(this IServiceCollection services, Action<ConfigurationOptions> configure)
    {
        services.AddOptions().Configure(configure);
        services.AddSingleton<RedisLocker>();
    }
}