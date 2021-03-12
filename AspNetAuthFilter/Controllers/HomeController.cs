using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetAuthFilter.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index()
        {
            return Json("Hello world");
        }

        [AllowAnonymous]
        public async Task<ActionResult> IndexAllowAnonymous()
        {
            return Json("Hello world");
        }
    }
}
