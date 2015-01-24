using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tyvj.DataModels;
using Tyvj.ViewModels;

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
            ViewBag.Judgers = null;
            var judgers = new List<vJudger>();
            foreach (var c in SignalR.JudgeHub.Online)
            {
                var user = (from u in DbContext.Users
                                where u.Username == c.Username
                                select u).Single();
                judgers.Add(new vJudger(user,c));
            }
            ViewBag.Judgers = judgers;
            if (requestContext.HttpContext.User != null && requestContext.HttpContext.User.Identity.IsAuthenticated)
            {
                ViewBag.CurrentUser = (from u in DbContext.Users
                                       where u.Username == requestContext.HttpContext.User.Identity.Name
                                       select u).Single();
                CurrentUser = ViewBag.CurrentUser;
            }
            else
            {
                ViewBag.CurrentUser = null;
            }
            ViewBag.News = (from n in DbContext.News
                            orderby n.Time descending
                            select n).Take(5).ToList();
            //检查缓存
            if (ContestController.ContestListCache == null || ContestController.ContestListCache.Count == 0)
                ContestController.RefreshContestListCache();
            if (HomeController.HomeTopicsCache == null || HomeController.HomeTopicsCache.Count == 0)
                HomeController.RefreshHomeTopicsCache();
            if (ProblemController.ProblemListCache == null || ProblemController.ProblemListCache.Count == 0)
                ProblemController.RefreshProblemListCache();
        }
        public bool IsMaster()
        {
            var ret = false;
            try
            {
                ret = ViewBag.CurrentUser.Role >= UserRole.Master;
            }
            catch { }
            return ret;
        }
        public User CurrentUser { get; set; }
        public ActionResult Message(string msg)
        {
            return RedirectToAction("Info", "Shared", new { msg = msg });
        }
    }
}