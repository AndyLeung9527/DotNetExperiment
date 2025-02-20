using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AspNetAuth.AuthenticationAndAuthorization.Controllers;

[Route("[controller]/[action]")]
[ApiController]
public class AccountController : ControllerBase
{
    [HttpGet]
    public Task<IActionResult> Login()
    {
        return Task.FromResult<IActionResult>(Content("请登录"));
    }

    [HttpPost]
    public async Task<IActionResult> Login([FromQuery] string userName)
    {
        // 根据注入的身份验证方案, 会进行写入cookie操作
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, userName)], CookieAuthenticationDefaults.AuthenticationScheme)),
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.Now.AddMinutes(30)
            });

        return Content("登录成功");
    }

    [HttpGet]
    [Authorize]// 只要身份验证通过就可以访问
    // [Authorize(Roles = "Admin,Guest")]// 只有角色为Admin或Guest的用户才能访问
    // [Authorize(Policy = "CustomPolicy")]// 只有满足CustomPolicy策略的用户才能访问
    // [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]// 指定身份验证方案
    public Task<IActionResult> Index()
    {
        var userName = HttpContext.User.Claims.FirstOrDefault(o => ClaimTypes.Name.Equals(o.Type))?.Value;

        return Task.FromResult<IActionResult>(Content($"用户：{userName}已登录"));
    }
}
