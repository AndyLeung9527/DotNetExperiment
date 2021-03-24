using AspNetAccessMongoDB.DataPersist;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetAccessMongoDB.Controllers
{
    public class HomeController:Controller
    {
        protected IMongoDBAccessor _mongoDBAccessor;
        public HomeController(IMongoDBAccessor mongoDBAccessor)
        {
            _mongoDBAccessor = mongoDBAccessor;
        }
        public ActionResult Index()
        {
            var obj = _mongoDBAccessor.Find("00000000000000000000000000000000");
            return Json(obj);
        }
    }
}
