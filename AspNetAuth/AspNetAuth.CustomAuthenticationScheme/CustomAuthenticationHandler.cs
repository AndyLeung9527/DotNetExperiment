using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace AspNetAuth.CustomAuthenticationScheme;

public class CustomAuthenticationHandler : IAuthenticationHandler
{
    private AuthenticationScheme? _scheme;
    private HttpContext? _httpContext;

    // 初始化
    public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
    {
        _scheme = scheme;
        _httpContext = context;
        return Task.CompletedTask;
    }

    // 身份验证处理
    public Task<AuthenticateResult> AuthenticateAsync()
    {
        // 从请求头中获取自定义的身份验证信息
        var authorization = _httpContext?.Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authorization))
        {
            return Task.FromResult(AuthenticateResult.NoResult());// 没有身份验证信息
        }

        // 解析自定义的身份验证信息
        if (!authorization.StartsWith("Custom ", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }
        var token = authorization.Substring("Custom ".Length).Trim();

        // 验证自定义的身份验证信息
        if (string.IsNullOrEmpty(token))
        {
            return Task.FromResult(AuthenticateResult.Fail("Token is null or empty."));// 身份验证信息为空
        }
        if (token != "Token")
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid token."));// 身份验证信息无效
        }

        // 创建身份验证票据
        var claimIdentity = new ClaimsIdentity(_scheme?.Name);
        claimIdentity.AddClaim(new Claim(ClaimTypes.Name, "CustomUser"));
        claimIdentity.AddClaim(new Claim(ClaimTypes.Role, "CustomRole"));
        var claimPrincipal = new ClaimsPrincipal(claimIdentity);
        var ticket = new AuthenticationTicket(claimPrincipal, _scheme?.Name ?? string.Empty);// 创建身份验证票据

        return Task.FromResult(AuthenticateResult.Success(ticket));// 身份验证成功
    }

    // 未登录处理
    public Task ChallengeAsync(AuthenticationProperties? properties)
    {
        _httpContext?.Response.Redirect("/Account/Login");

        return Task.CompletedTask;
    }

    // 未授权（权限）处理
    public Task ForbidAsync(AuthenticationProperties? properties)
    {
        if (_httpContext != null)
        {
            _httpContext.Response.StatusCode = 403;
        }

        return Task.CompletedTask;
    }
}

public class CustomAuthenticationDefaults
{
    // 提供身份验证方案名称
    public const string AuthenticationScheme = "CustomScheme";
}
