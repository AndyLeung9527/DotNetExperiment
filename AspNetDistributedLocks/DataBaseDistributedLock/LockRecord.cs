namespace DatabaseDistributedLock;

public class LockRecord
{
    /// <summary>
    /// 锁名
    /// </summary>
    public string Key { get; private set; } = null!;

    /// <summary>
    /// 唯一标识
    /// </summary>
    public string Id { get; private set; } = null!;

    /// <summary>
    /// 重入计数
    /// </summary>
    public uint Count { get; private set; }

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTimeOffset ExpireTime { get; private set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreationTime { get; private set; }

    public LockRecord(string key, string id, uint count, DateTimeOffset expireTime, DateTimeOffset creationTime)
    {
        Key = key;
        Id = id;
        Count = count;
        ExpireTime = expireTime;
        CreationTime = creationTime;
    }

    public void SetExpireTime(DateTimeOffset expireTime)
    {
        ExpireTime = expireTime;
    }

    //public void SetCreationTime(DateTimeOffset creationTime)
    //{
    //    CreationTime = creationTime;
    //}

    /// <summary>
    /// 是否已过期
    /// </summary>
    /// <returns></returns>
    public bool Expired()
    {
        return DateTimeOffset.Now > ExpireTime;
    }
}