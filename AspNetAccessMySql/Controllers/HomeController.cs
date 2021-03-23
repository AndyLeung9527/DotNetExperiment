using AspNetAccessMySql.DbContexts;
using AspNetAccessMySql.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetAccessMySql.Controllers
{
    public class HomeController:Controller
    {
        private readonly erp_css _erp_css;
        public HomeController(erp_css erp_css)
        {
            _erp_css = erp_css;
        }
        public ActionResult Index()
        {
            var list = _erp_css.Set<test_user>().ToList();
            return Json(list);
        }
    }
}
