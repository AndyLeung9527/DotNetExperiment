namespace RedisDistributedLock;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Collections.Concurrent;

public class RedisLocker : IDisposable
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly string _acquireScript;
    private readonly ConcurrentDictionary<RenewalKey, Timer> _renewals;
    private readonly string _renewalScript;

    public RedisLocker(IOptions<ConfigurationOptions> options)
    {
        _connectionMultiplexer = ConnectionMultiplexer.Connect(options.Value);
        _acquireScript = """
            -- 若锁不存在，则新增锁并设置：1.唯一标识，2.重入计数(初始值为1)，3.锁过期时间
            if (redis.call('exists', @key) == 0) then
                redis.call('hset', @key, @id, 1);
                redis.call('pexpire', @key, @expire);
                return 0;
            end;
            -- 若锁存在且唯一标识匹配，设置：1.重入计数+1，2.锁过期时间
            if (redis.call('hexists', @key, @id) == 1) then
                redis.call('hincrby', @key, @id, 1);
                redis.call('pexpire', @key, @expire);
                return 1;
            end;
            -- 新增锁返回0，重入锁返回1，获取失败返回null
            return nil;
            """;
        _renewals = new();
        _renewalScript = """
            -- 若锁存在则重新设置过期时间
            if (redis.call('hexists', @key, @id) == 1) then
                redis.call('pexpire', @key, @expire);
                return 1;
            end;
            return 0;
            """;
    }

    /// <summary>
    /// 获取锁
    /// </summary>
    /// <param name="key">锁名</param>
    /// <param name="id">唯一标识</param>
    /// <param name="expire">过期时长</param>
    /// <param name="autoRenewal">自动续约(仅新增锁有效)</param>
    /// <returns></returns>
    public async Task<Locker> LockAsync(string key, string id, TimeSpan expire, bool autoRenewal = false, CancellationToken cancellationToken = default)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var acquireResult = await _connectionMultiplexer.GetDatabase(default).ScriptEvaluateAsync(LuaScript.Prepare(_acquireScript), new
            {
                key,
                id,
                expire = Convert.ToInt64(expire.TotalMilliseconds),
            });
            if (!acquireResult.IsNull)
            {
                if ((int)acquireResult == 0)
                {
                    var ctsAutoRenewal = new CancellationTokenSource();
                    if (autoRenewal)
                    {
                        var renewalKey = new RenewalKey(key, id);
                        var renewal = new Renewal(renewalKey, expire, ctsAutoRenewal.Token);
                        var renewalTimer = new Timer(async (o) => await RenewalAsync(o), renewal, expire / 2, expire / 2);//每隔一半的过期时间则执行续约(仅新增锁)
                        _renewals.AddOrUpdate(renewalKey, renewalTimer, (k, t) =>
                        {
                            t?.Dispose();
                            return renewalTimer;
                        });
                    }
                    return new Locker(key, id, expire, _connectionMultiplexer, ctsAutoRenewal);
                }
                if ((int)acquireResult == 1) return new Locker(key, id, expire, _connectionMultiplexer, new());
            }
        }
    }

    public void Dispose()
    {
        _connectionMultiplexer.Dispose();
    }

    /// <summary>
    /// 续约(重新设定过期时间)
    /// </summary>
    /// <returns></returns>
    private async Task RenewalAsync(object? state)
    {
        var renewal = state as Renewal;
        if (renewal == null) return;

        if (renewal.CancellationToken.IsCancellationRequested)
        {
            _renewals.TryRemove(renewal.RenewalKey, out var timer);
            timer?.Dispose();
            return;
        }

        var renewalResult = await _connectionMultiplexer.GetDatabase(default).ScriptEvaluateAsync(LuaScript.Prepare(_renewalScript), new
        {
            key = renewal.RenewalKey.Key,
            id = renewal.RenewalKey.Id,
            expire = Convert.ToInt64(renewal.Expire.TotalMilliseconds),
        });
        if (renewalResult.IsNull || (int)renewalResult == 0)
        {
            _renewals.TryRemove(renewal.RenewalKey, out var timer);
            timer?.Dispose();
        }
    }

    record RenewalKey(string Key, string Id);
    record Renewal(RenewalKey RenewalKey, TimeSpan Expire, CancellationToken CancellationToken);
}

public class Locker : IDisposable
{
    public string Key { get; }

    public string Id { get; }

    public TimeSpan Expire { get; }

    private readonly IConnectionMultiplexer _connectionMultiplexer;

    private readonly CancellationTokenSource _ctsAutoRenewal;

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

    public Locker(string key, string id, TimeSpan expire, IConnectionMultiplexer connectionMultiplexer, CancellationTokenSource ctsAutoRenewal)
    {
        Key = key;
        Id = id;
        Expire = expire;
        _connectionMultiplexer = connectionMultiplexer;
        _ctsAutoRenewal = ctsAutoRenewal;
    }

    /// <summary>
    /// 释放锁
    /// </summary>
    /// <returns>item1: 是否成功, item2: 锁是否被完全释放</returns>
    /// <exception cref="ObjectDisposedException"></exception>
    public async Task<(bool Succeeded, bool Released)> ReleaseAsync()
    {
        if (!_ctsAutoRenewal.IsCancellationRequested)
        {
            var releaseResult = await _connectionMultiplexer.GetDatabase(default).ScriptEvaluateAsync(LuaScript.Prepare(RELEASE_SCRIPT), new
            {
                key = Key,
                id = Id,
                expire = Convert.ToInt64(Expire.TotalMilliseconds)
            });

            bool succeed = !releaseResult.IsNull;
            bool released = releaseResult.IsNull ? false : (int)releaseResult == 1;
            if (released)
            {
                await _ctsAutoRenewal.CancelAsync();
                _ctsAutoRenewal.Dispose();
            }

            return (succeed, released);
        }
        throw new ObjectDisposedException($"重复释放, Key: {Key}, Id: {Id}");
    }

    public void Dispose()
    {
        if (!_ctsAutoRenewal.IsCancellationRequested)
        {
            _ctsAutoRenewal.Cancel();
            _ctsAutoRenewal.Dispose();
            _connectionMultiplexer.GetDatabase(default).ScriptEvaluate(LuaScript.Prepare(RELEASE_SCRIPT), new
            {
                key = Key,
                id = Id,
                expire = Convert.ToInt64(Expire.TotalMilliseconds)
            });
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