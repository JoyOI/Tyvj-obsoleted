using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Tyvj.Controllers
{
    public class SharedController : BaseController
    {
        public ActionResult Info(string msg)
        {
            ViewBag.Message = msg;
            return View();
        }
	}
}