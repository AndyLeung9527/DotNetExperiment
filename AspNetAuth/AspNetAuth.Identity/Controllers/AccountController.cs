using AspNetAuth.Identity.Dtos;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AspNetAuth.Identity.Controllers;

[Route("[controller]/[action]")]
[ApiController]
public class AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager) : ControllerBase
{
    // UserManager、SignInManager用户管理服务, 提供增删改查等, AddIdentity()时被注入
    private readonly UserManager<IdentityUser> _userManager = userManager;
    private readonly SignInManager<IdentityUser> _signInManager = signInManager;

    // 注册用户
    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var user = new IdentityUser(dto.UserName);
        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            return BadRequest(string.Join(", ", result.Errors.Select(o => o.Description)));
        }

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        return Content($"注册成功, 邮箱确认令牌:{token}");
    }

    // 确认邮箱
    [HttpGet]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string userName, [FromQuery] string token)
    {
        var user = await _userManager.FindByNameAsync(userName);
        if (user == null)
        {
            return BadRequest("用户不存在");
        }

        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (!result.Succeeded)
        {
            return BadRequest(string.Join(", ", result.Errors.Select(o => o.Description)));
        }

        // 用户设定为需要两步验证
        await _userManager.SetTwoFactorEnabledAsync(user, true);

        return Content("邮箱确认成功");
    }

    // 登录页面
    [HttpGet]
    public Task<IActionResult> Login()
    {
        return Task.FromResult<IActionResult>(Content("登录页面"));
    }

    // 手动登录, 不建议
    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _userManager.FindByNameAsync(dto.UserName);
        if (user != null && await _userManager.CheckPasswordAsync(user, dto.Password))
        {
            var identity = new ClaimsIdentity(IdentityConstants.ApplicationScheme/*Microsoft.AspNetCore.Identity默认身份验证方案名*/);
            identity.AddClaim(new Claim(ClaimTypes.Name, user.UserName ?? string.Empty));
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, role));
            }

            // 根据注入的身份验证方案, 会进行写入cookie操作
            await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme/*Microsoft.AspNetCore.Identity默认身份验证方案名*/, new ClaimsPrincipal(identity));

            return Content("登录成功");
        }

        return BadRequest("无效的用户名或密码");
    }

    // 自动登录
    [HttpPost]
    public async Task<IActionResult> Login_V2([FromBody] LoginDto dto)
    {
        var result = await _signInManager.PasswordSignInAsync(dto.UserName, dto.Password, true, lockoutOnFailure: true/*是否启用登录失败锁定功能*/);

        if (result.Succeeded)
        {
            return Content("登录成功");
        }
        if (result.IsLockedOut)
        {
            return Content("账户由于多次登录失败已锁定");
        }

        // 若用户需要两步验证, 如果用户邮箱为空则无效, 会跳过两步验证直接登录成功
        if (result.RequiresTwoFactor)
        {
            var user = await _userManager.FindByNameAsync(dto.UserName);
            if (user == null)
            {
                return BadRequest("用户不存在");
            }
            var providers = await _userManager.GetValidTwoFactorProvidersAsync(user);
            if (!providers.Contains("Email"))
            {
                return BadRequest("未配置邮箱两步验证");
            }
            var token = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
            return Content($"用户登录需要两步验证, token:{token}");
        }

        return BadRequest("无效的用户名或密码");
    }

    // 两步验证登录
    [HttpPost]
    public async Task<IActionResult> LoginTwoStep([FromBody] LoginTwoStepDto dto)
    {
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            return BadRequest("用户不存在");
        }

        var result = await _signInManager.TwoFactorSignInAsync("Email", dto.Token, false, false);
        if (result.Succeeded)
        {
            return Content("两步登录成功");
        }
        if (result.IsLockedOut)
        {
            return Content("账户由于多次登录失败已锁定");
        }

        return BadRequest("无效的两步登录");
    }

    // 登出
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        // 删除cookie
        await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme/*Microsoft.AspNetCore.Identity默认身份验证方案名*/);
        return Content("登出成功");
    }

    // 忘记密码
    [HttpPost]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        var user = await _userManager.FindByNameAsync(dto.UserName);
        if (user == null)
        {
            return BadRequest("用户不存在");
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        return Content($"密码重置令牌:{token}");
    }

    // 重置密码
    [HttpPost]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var user = await _userManager.FindByNameAsync(dto.UserName);
        if (user == null)
        {
            return BadRequest("用户不存在");
        }

        var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
        if (!result.Succeeded)
        {
            return BadRequest(string.Join(", ", result.Errors.Select(o => o.Description)));
        }

        return Content("密码重置成功");
    }
}
