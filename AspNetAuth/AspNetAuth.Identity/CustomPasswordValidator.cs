using Microsoft.AspNetCore.Identity;

namespace AspNetAuth.Identity;

public class CustomPasswordValidator<TUser> : IPasswordValidator<TUser> where TUser : IdentityUser
{
    public async Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user, string? password)
    {
        var userName = await manager.GetUserNameAsync(user);
        if (userName == password)
        {
            return IdentityResult.Failed(new IdentityError
            {
                Code = "PasswordEqualsUserName",
                Description = "密码不能与用户名相同"
            });
        }

        return IdentityResult.Success;
    }
}
