using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AspNetAuth.SSO.AuthorizationServer.Controllers;

[Route("[controller]/[action]")]
[ApiController]
public class JwtController(IOptionsSnapshot<JwtTokenOptions> options) : ControllerBase
{
    private readonly JwtTokenOptions _options = options.Value;

    // 获取访问令牌，使用对称密钥
    [HttpGet]
    public Task<IActionResult> GetJwtTokenBySymmetrickey([FromQuery] string name, [FromQuery] string password)
    {
        if ("zhangsan".Equals(name) && "123456".Equals(password))
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, name),
                new Claim(ClaimTypes.Role, "Guest")
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecurityKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                notBefore: DateTime.Now,// 立即生效
                signingCredentials: creds);
            var writeToken = new JwtSecurityTokenHandler().WriteToken(token);

            return Task.FromResult<IActionResult>(new JsonResult(new
            {
                Result = true,
                Token = writeToken
            }));
        }

        return Task.FromResult<IActionResult>(new JsonResult(new
        {
            Result = false
        }));
    }

    // 获取访问令牌，使用非对称密钥
    [HttpGet]
    public async Task<IActionResult> GetJwtTokenByAsymmetrickey([FromQuery] string name, [FromQuery] string password)
    {
        // RSA密钥对
        string privateKeyPem;
        string publicKeyPem;

        var currentDir = Directory.GetCurrentDirectory();
        var privateKeyPath = Path.Combine(currentDir, "private.pem");
        var publicKeyPath = Path.Combine(currentDir, "public.pem");
        if (!System.IO.File.Exists(privateKeyPath) || !System.IO.File.Exists(publicKeyPath))
        {
            System.IO.File.Delete(privateKeyPath);
            System.IO.File.Delete(publicKeyPath);
            (privateKeyPem, publicKeyPem) = RsaKeyGenerator.GenerateRsaKeys();
            await System.IO.File.WriteAllTextAsync(privateKeyPath, privateKeyPem);
            await System.IO.File.WriteAllTextAsync(publicKeyPath, publicKeyPem);
        }
        else
        {
            privateKeyPem = System.IO.File.ReadAllText(privateKeyPath);
            publicKeyPem = System.IO.File.ReadAllText(publicKeyPath);
        }

        if ("zhangsan".Equals(name) && "123456".Equals(password))
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, name),
                new Claim(ClaimTypes.Role, "Guest")
            };
            var key = new RsaSecurityKey(RsaKeyGenerator.ConvertStringToRSAParameters(privateKeyPem));
            var creds = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);
            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                notBefore: DateTime.Now,// 立即生效
                signingCredentials: creds);
            var writeToken = new JwtSecurityTokenHandler().WriteToken(token);
            return new JsonResult(new
            {
                Result = true,
                Token = writeToken
            });
        }
        return new JsonResult(new
        {
            Result = false
        });
    }

    // 存放被生成过的刷新令牌和对应的用户信息, 真正生产环境中应该存放在数据库或者缓存中，并设置过期时间
    // 用户信息变更时可以主动删除对应的刷新令牌使其失效
    private static Dictionary<string, (string, string)> _refreshTokens = new();
    // 获取访问令牌和刷新令牌，可使用对称密钥或非对称密钥，示例中使用对称密钥
    [HttpGet]
    public Task<IActionResult> GetAccessTokenAndRefreshToken([FromQuery] string name, [FromQuery] string password)
    {
        var accessToken = GenToken(name, password, TimeSpan.FromHours(1));
        var refreshToken = GenToken(name, password, TimeSpan.FromDays(7));
        _refreshTokens[refreshToken] = (name, password);

        return Task.FromResult<IActionResult>(new JsonResult(new
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        }));
    }
    // 刷新访问令牌
    [HttpGet]
    public Task<IActionResult> RefreshAccessToken([FromQuery] string refreshToken)
    {
        //Refresh token是否过期
        var payload = new JwtSecurityTokenHandler().ReadJwtToken(refreshToken).Payload;
        if (DateTimeOffset.Now > DateTimeOffset.FromUnixTimeSeconds(payload?.Expiration ?? default))
        {
            return Task.FromResult<IActionResult>(new JsonResult(new
            {
                Result = false,
                Message = "Refresh token has expired"
            }));
        }

        // Refresh token是否存在(是否有效)
        if (_refreshTokens.TryGetValue(refreshToken, out var user))
        {
            var accessToken = GenToken(user.Item1, user.Item2, TimeSpan.FromHours(1));
            return Task.FromResult<IActionResult>(new JsonResult(new
            {
                Result = true,
                AccessToken = accessToken
            }));
        }

        return Task.FromResult<IActionResult>(new JsonResult(new
        {
            Result = false
        }));
    }

    private string GenToken(string name, string password, TimeSpan expire)
    {
        var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, name),
                new Claim(ClaimTypes.Role, "Guest")
            };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecurityKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.Now.Add(expire),
            notBefore: DateTime.Now,// 立即生效
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public static class RsaKeyGenerator
{
    public static (string privateKey, string publicKey) GenerateRsaKeys()
    {
        using var rsa = RSA.Create(2048);
        return (rsa.ExportRSAPrivateKeyPem(), rsa.ExportRSAPublicKeyPem());
    }

    public static RSAParameters ConvertStringToRSAParameters(string privateKeyPem)
    {
        using var rsa = RSA.Create();
        rsa.ImportFromPem(privateKeyPem);
        return rsa.ExportParameters(true);
    }
}