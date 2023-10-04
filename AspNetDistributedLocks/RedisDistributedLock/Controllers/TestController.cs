namespace RedisDistributedLock.Controllers;

using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

[Route("api/[controller]/[action]")]
[ApiController]
public class TestController : ControllerBase
{
    const string _stockKey = "StockNum";
    const string _lockKey = "StockLock";
    static TimeSpan _tsExpired = TimeSpan.FromSeconds(3);
    readonly ConnectionMultiplexer _redisConn;
    public TestController(ConnectionMultiplexer redisConn)
    {
        _redisConn = redisConn;
    }

    [HttpGet("{stockNum}")]
    public async Task<IActionResult> InitializeStock([FromRoute] long stockNum)
    {
        var result = await _redisConn.GetDatabase(0).StringSetAsync(_stockKey, stockNum);
        return Content(result.ToString());
    }

    [HttpGet]
    public async Task<IActionResult> Consume()
    {
        var stockNum = (long)(await _redisConn.GetDatabase(0).StringGetAsync(_stockKey));
        if (stockNum <= 0)
            return BadRequest("None stock");

        var guid = Guid.NewGuid();
        while (!await _redisConn.GetDatabase(0).StringSetAsync(_lockKey, guid.ToString(), _tsExpired, When.NotExists))
            await Task.Delay(300);

        string script = string.Empty;
        await _redisConn.GetDatabase(0).ScriptEvaluateAsync(script);
        stockNum = (long)await _redisConn.GetDatabase(0).StringGetAsync(_stockKey);
        if (stockNum > 0)
        {
            stockNum -= 1;
            var result = await _redisConn.GetDatabase(0).StringSetAsync(_stockKey, stockNum);
            return Content(result.ToString());
        }


        return BadRequest("None stock");
    }
}