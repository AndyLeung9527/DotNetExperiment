using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AspNetAuth.Controllers
{
    public class CookieLoginController:Controller
    {
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Index(string account,string password)
        {
            if("AndyLeung".Equals(account) && "123456".Equals(password))
            {
                IList<Claim> lstClaims = new List<Claim>
                {
                     new Claim(ClaimTypes.Name,account),
                     new Claim("Password",password)
                };
                ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(lstClaims, CookieAuthenticationDefaults.AuthenticationScheme));
                await HttpContext.SignInAsync(claimsPrincipal, new AuthenticationProperties { ExpiresUtc = DateTime.UtcNow.AddMinutes(30) });

                return Json("Login succeed.");
            }

            return Json("Account or password wrong.");
        }
    }
}
