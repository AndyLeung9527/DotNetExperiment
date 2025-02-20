using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AspNetAuth.CustomAuthorizationPolicy.Controllers;

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
    public async Task<IActionResult> Login([FromQuery] string userName, [FromQuery] string email)
    {
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, userName), new Claim(ClaimTypes.Email, email)], CookieAuthenticationDefaults.AuthenticationScheme)),
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.Now.AddMinutes(30)
            });

        return Content("登录成功");
    }

    [HttpGet]
    [Authorize(Policy = "CustomPolicy")]
    public Task<IActionResult> Index()
    {
        var userName = HttpContext.User.Claims.FirstOrDefault(o => ClaimTypes.Name.Equals(o.Type))?.Value;

        return Task.FromResult<IActionResult>(Content($"用户：{userName}已登录"));
    }
}
