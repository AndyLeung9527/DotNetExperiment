using System.ComponentModel.DataAnnotations;

namespace AspNetAuth.Identity.Dtos;

public class RegisterDto
{
    [Required(ErrorMessage = "用户名必填")]
    [StringLength(50, ErrorMessage = "用户名长度不能超过50个字符")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "密码必填")]
    [StringLength(30, MinimumLength = 6, ErrorMessage = "密码长度必须在6到30个字符之间")]
    public string Password { get; set; } = string.Empty;
}
