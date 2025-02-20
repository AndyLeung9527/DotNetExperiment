using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AspNetAuth.CustomAuthorizationPolicy;

public class CustomAuthorizationRequirement : IAuthorizationRequirement
{
}

public class Mail163Handler : AuthorizationHandler<CustomAuthorizationRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CustomAuthorizationRequirement requirement)
    {
        if (context.User != null && context.User.HasClaim(c => c.Type == ClaimTypes.Email))
        {
            var emailClaims = context.User.FindAll(c => c.Type == ClaimTypes.Email);// 所有Scheme
            if (emailClaims.Any(c => c.Value.EndsWith("@163.com", StringComparison.OrdinalIgnoreCase)))
            {
                context.Succeed(requirement);
            }
        }

        return Task.CompletedTask;
    }
}

public class MailQqHandler : AuthorizationHandler<CustomAuthorizationRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CustomAuthorizationRequirement requirement)
    {
        if (context.User != null && context.User.HasClaim(c => c.Type == ClaimTypes.Email))
        {
            var emailClaims = context.User.FindAll(c => c.Type == ClaimTypes.Email);// 所有Scheme
            if (emailClaims.Any(c => c.Value.EndsWith("@qq.com", StringComparison.OrdinalIgnoreCase)))
            {
                context.Succeed(requirement);
            }
        }

        return Task.CompletedTask;
    }
}