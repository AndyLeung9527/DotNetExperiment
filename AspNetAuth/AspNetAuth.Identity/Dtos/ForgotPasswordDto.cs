using System.ComponentModel.DataAnnotations;

namespace AspNetAuth.Identity.Dtos;

public class ForgotPasswordDto
{
    [Required(ErrorMessage = "用户名必填")]
    [StringLength(50, ErrorMessage = "用户名长度不能超过50个字符")]
    public string UserName { get; set; } = string.Empty;
}

