using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Tyvj.Controllers
{
    public class SharedController : BaseController
    {
        public ActionResult Message(string msg)
        {
            ViewBag.Message = msg;
            return View();
        }
	}
}