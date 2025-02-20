using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AspNetAuth.MultipleAuthenticationSchemes.Controllers;

[Route("[controller]/[action]")]
[ApiController]
public class AccountController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Login_1()
    {
        await HttpContext.SignInAsync($"{CookieAuthenticationDefaults.AuthenticationScheme}_1", new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.Name, "user1"),
            new Claim(ClaimTypes.Role, "admin1")
        }, $"{CookieAuthenticationDefaults.AuthenticationScheme}_1")));

        return Content("登录方式1成功");
    }

    [HttpGet]
    public async Task<IActionResult> Login_2()
    {
        await HttpContext.SignInAsync($"{CookieAuthenticationDefaults.AuthenticationScheme}_2", new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.Name, "user2"),
            new Claim(ClaimTypes.Role, "admin2")
        }, $"{CookieAuthenticationDefaults.AuthenticationScheme}_2")));
        return Content("登录方式2成功");
    }

    // 用户信息可以来自不同的方案, 即授权支持多个Scheme
    // 当指定默认的方案后, 对应的AuthenticateAsync在框架中会自动执行
    [HttpGet]
    // [Authorize(AuthenticationSchemes = $"{CookieAuthenticationDefaults.AuthenticationScheme}_1, {CookieAuthenticationDefaults.AuthenticationScheme}_2")]
    public async Task<IActionResult> Index()
    {
        // 根据方案执行身份验证, 未执行以上对应的SignInAsync操作时, 会返回null
        var result1 = await HttpContext.AuthenticateAsync($"{CookieAuthenticationDefaults.AuthenticationScheme}_1");
        var result2 = await HttpContext.AuthenticateAsync($"{CookieAuthenticationDefaults.AuthenticationScheme}_2");

        var user1 = result1?.Principal?.Claims?.FirstOrDefault(o => o.Type == ClaimTypes.Name)?.Value;
        var user2 = result2?.Principal?.Claims?.FirstOrDefault(o => o.Type == ClaimTypes.Name)?.Value;

        return Content($"登录方式1是否已登录: {result1?.Succeeded}, 用户名: {user1}. 登录方式2是否已登录: {result2?.Succeeded}, 用户名: {user2}");
    }
}
