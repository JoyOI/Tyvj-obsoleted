using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Tyvj.Controllers
{
    public class ForumController : BaseController
    {

        //
        // GET: /Forum/
        public ActionResult Index(int? id)
        {
            var forum = DbContext.Forums.Find(id);
            ViewBag.ForumList = (from f in DbContext.Forums
                                 where f.FatherID ==null
                                 orderby f.Sort ascending
                                 select f).ToList();
            return View(forum);
        }
	}
}