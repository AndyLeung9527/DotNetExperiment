namespace RedisDistributedLock.Controllers;

using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

[Route("api/[controller]/[action]")]
[ApiController]
public class TestController : ControllerBase
{
    const string _stockKey = "StockNum";
    const string _lockKey = "StockLock";
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
        if (stockNum > 0)
        {
            var redisLock = _redisConn.GetDatabase(0).KeyExistsAsync()
            stockNum -= 1;
            var result = await _redisConn.GetDatabase(0).StringSetAsync(_stockKey, stockNum);
            return Content(result.ToString());
        }

        return BadRequest("None stock");
    }
}