namespace DatabaseDistributedLock;

using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

public class DatabaseLocker : IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _interval;
    private readonly ConcurrentDictionary<RenewalKey, Timer> _renewals;

    public DatabaseLocker(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _interval = TimeSpan.FromMicroseconds(100);
        _renewals = new();
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
            using var scope = _serviceProvider.CreateScope();
            using var lockDbContext = scope.ServiceProvider.GetRequiredService<LockDbContext>();
            var now = DateTimeOffset.Now;
            var expireTime = now.Add(expire);

            //若存在过期锁则直接删除
            await lockDbContext.LockRecords.Where(o => o.Key == key && o.ExpireTime < now).ExecuteDeleteAsync();

            //若锁存在且唯一标识匹配，设置：1.重入计数+1，2.锁过期时间
            var updateRows = await lockDbContext.LockRecords
                .Where(o => o.Key == key && o.Id == id && o.ExpireTime >= now)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(b => b.Count, b => b.Count + 1)
                        .SetProperty(b => b.ExpireTime, expireTime));
            if (updateRows > 0) return new Locker(key, id, expireTime, _serviceProvider, new());
            //若锁不存在，则插入锁并设置：1.唯一标识，2.重入计数(初始值为1)，3.锁过期时间
            else if (updateRows <= 0)
            {
                try
                {
                    lockDbContext.LockRecords.RemoveRange(lockDbContext.LockRecords.Where(o => o.Key == key));

                    await lockDbContext.LockRecords.AddAsync(new LockRecord(key, id, 1, expireTime, now));

                    await lockDbContext.SaveChangesAsync();
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
                    return new Locker(key, id, expireTime, _serviceProvider, ctsAutoRenewal);
                }
                catch
                {
                    await Task.Delay(_interval);
                    continue;
                }
            }
        }
    }

    public void Dispose()
    {
        foreach (var key in _renewals.Keys)
        {
            _renewals.TryRemove(key, out var timer);
            timer?.Dispose();
        }
    }

    /// <summary>
    /// 续约(原过期时间加一半)
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

        using var scope = _serviceProvider.CreateScope();
        using var lockDbContext = scope.ServiceProvider.GetRequiredService<LockDbContext>();
        var lockRecord = await lockDbContext.LockRecords.FirstOrDefaultAsync(o => o.Key == renewal.RenewalKey.Key && o.Id == renewal.RenewalKey.Id);
        if (lockRecord?.Expired() is false)
        {
            lockRecord.SetExpireTime(lockRecord.ExpireTime.Add(renewal.Expire / 2));
            try
            {
                await lockDbContext.SaveChangesAsync();
            }
            catch { }
        }
        else
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
    public string Key { get; } = null!;

    public string Id { get; } = null!;

    public DateTimeOffset ExpireTime { get; }

    private readonly IServiceProvider _serviceProvider;

    private readonly CancellationTokenSource _ctsAutoRenewal;

    public Locker(string key, string id, DateTimeOffset expireTime, IServiceProvider serviceProvider, CancellationTokenSource ctsAutoRenewal)
    {
        Key = key;
        Id = id;
        ExpireTime = expireTime;
        _serviceProvider = serviceProvider;
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
            using var scope = _serviceProvider.CreateScope();
            using var lockDbContext = scope.ServiceProvider.GetRequiredService<LockDbContext>();

            var updatedRows = 0;
            var deletedRows = 0;
            try
            {
                //若锁存在且唯一标识匹配和重入计数大于0，则重入计数-1
                updatedRows = await lockDbContext.LockRecords
                        .Where(o => o.Key == Key && o.Id == Id && o.Count > 0)
                            .ExecuteUpdateAsync(setters => setters
                                .SetProperty(b => b.Count, b => b.Count - 1));

                //若锁存在且唯一标识匹配但重入计数小于等于0，则删除锁
                deletedRows = await lockDbContext.LockRecords
                        .Where(o => o.Key == Key && o.Id == Id && o.Count <= 0)
                            .ExecuteDeleteAsync();
            }
            catch { }

            bool succeed = updatedRows > 0;
            bool released = deletedRows > 0;
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
        }
    }
}

public static class DatabaseLockerExtensions
{
    public static void AddDatabaseLocker(this IServiceCollection services, Action<DbContextOptionsBuilder>? optionsAction = null, ServiceLifetime contextLifetime = ServiceLifetime.Scoped, ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
    {
        services.AddDbContext<LockDbContext>(optionsAction, contextLifetime, optionsLifetime);
        services.AddSingleton<DatabaseLocker>();
    }

    public static void UseDatabaseLocker(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var lockDbContext = scope.ServiceProvider.GetRequiredService<LockDbContext>();
        lockDbContext.Database.EnsureCreated();
    }
}