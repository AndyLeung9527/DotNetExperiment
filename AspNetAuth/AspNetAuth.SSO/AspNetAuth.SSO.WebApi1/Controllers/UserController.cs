using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AspNetAuth.SSO.WebApi1.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    [HttpGet]
    [Authorize]
    public Task<IActionResult> Get()
    {
        var userName = HttpContext.User.Claims.FirstOrDefault(o => ClaimTypes.Name.Equals(o.Type))?.Value;

        return Task.FromResult<IActionResult>(Content($"用户：{userName}已登录"));
    }
}
