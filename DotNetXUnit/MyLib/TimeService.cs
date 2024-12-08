namespace MyLib;

//使用TimeProvider, 方便在单元测试中模拟不同的时间进行验证
public class TimeService
{
    private readonly TimeProvider _timeProvider;

    public TimeService(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public int GetMonths(DateTimeOffset dateTimeOffset)
    {
        var utcNow = _timeProvider.GetUtcNow();
        if (dateTimeOffset > utcNow)
        {
            throw new ArgumentException("The date must be in the past", nameof(dateTimeOffset));
        }

        return utcNow.Month - dateTimeOffset.Month;
    }
}
