using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetAuthFilter.Filters
{
    public class AuthorizationFilter : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var controllerActionDescriptor = context?.ActionDescriptor as ControllerActionDescriptor;

            var boolAllowAnonymousClass = controllerActionDescriptor?.ControllerTypeInfo?.GetCustomAttributes(inherit: true).Any(a => a.GetType().Equals(typeof(AllowAnonymousAttribute)));
            if (boolAllowAnonymousClass.HasValue && boolAllowAnonymousClass.Value)
                return;

            var boolAllowAnonymousMethod = controllerActionDescriptor?.MethodInfo?.GetCustomAttributes(inherit: true).Any(a => a.GetType().Equals(typeof(AllowAnonymousAttribute)));
            if (boolAllowAnonymousMethod.HasValue && boolAllowAnonymousMethod.Value)
                return;

            context.Result = new Microsoft.AspNetCore.Mvc.JsonResult("No permissions");
        }
    }
}
