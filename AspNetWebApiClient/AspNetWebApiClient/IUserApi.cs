namespace AspNetWebApiClient;

using Lib;
using WebApiClientCore.Attributes;

[LoggingFilter]
public interface IUserApi
{
    [HttpGet("api/users/{id}")]
    Task<User> GetAsync(string id, CancellationToken token = default);

    [HttpPost("api/users")]
    Task<User> PostAysnc([JsonContent] User user, CancellationToken token = default);
}
