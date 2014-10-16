using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tyvj.DataModels;

namespace Tyvj.Controllers
{
    public class BaseController : Controller
    {
        public readonly DB DbContext = new DB();
        public BaseController()
        {

        }
        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);
            if (requestContext.HttpContext.User != null && requestContext.HttpContext.User.Identity.IsAuthenticated)
            {
                ViewBag.CurrentUser = (from u in DbContext.Users
                                       where u.Username == requestContext.HttpContext.User.Identity.Name
                                       select u).Single();
            }
            else
            {
                ViewBag.CurrentUser = null;
            }
            ViewBag.News = (from n in DbContext.News
                            orderby n.Time descending
                            select n).Take(5).ToList();
        }
    }
}