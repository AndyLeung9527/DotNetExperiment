using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AspNetAuth.CustomAuthenticationScheme.Controllers;

[Route("[controller]/[action]")]
[ApiController]
public class AccountController : ControllerBase
{
    [HttpGet]
    public IActionResult Login()
    {
        return Content("登录页面");
    }

    [HttpGet]
    [Authorize]
    public IActionResult Index()
    {
        var userName = HttpContext.User.Claims.FirstOrDefault(o => o.Type == ClaimTypes.Name)?.Value;
        return Content($"用户：{userName}已登录");
    }
}

