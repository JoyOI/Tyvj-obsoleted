using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Tyvj.Controllers
{
    public class AdminController : BaseController
    {
        // GET: Admin
        [Authorize]
        public ActionResult Index()
        {
            if (!IsMaster())
                return Message("您无权执行本操作");
            return View();
        }

        [Authorize]
        public ActionResult Users(int? page, string o)
        {
            if (!IsMaster())
                return Message("您无权执行本操作");
            if (page == null) page = 0;
            IEnumerable<Tyvj.DataModels.User> users = (from u in DbContext.Users
                         orderby u.ID ascending
                         select u);
            if (o == "Role")
                users = users.OrderByDescending(x => x.RoleAsInt);
            if (o == "Login")
                users = users.OrderByDescending(x=>x.LastLoginTime);
            if (o == "Register")
                users = users.OrderByDescending(x => x.RegisterTime);
            if (o == "School")
                users = users.OrderByDescending(x => x.School);
            users = users.Skip(page.Value * 100).Take(100).ToList();
            ViewBag.UserCount = DbContext.Users.Count();
            ViewBag.PageIndex = page.Value;
            return View(users);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Users(string username)
        {
            if (!IsMaster())
                return Message("您无权执行本操作");
            var user = (from u in DbContext.Users
                        where u.Username == username
                        select u).FirstOrDefault();
            if (user == null) return Message("没有找到这个用户");
            return RedirectToAction("Settings", "User", new { id = user.ID });
        }

        [Authorize]
        public ActionResult Rating()
        {
            if (!IsMaster())
                return Message("您无权执行本操作");
            var contests = (from c in ContestController.ContestListCache
                            where c.Official
                            && c.End <= DateTime.Now
                            && (from r in DbContext.Ratings
                                    where r.ContestID == c.ID
                                select r).Count() == 0
                            select c).ToList();
            return View(contests);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Rating(int ContestID)
        {
             if (!IsMaster())
                return Content("False");
             System.Threading.Tasks.Task.Factory.StartNew(() => { Helpers.Ratings.RatingCount(ContestID); });
            return Content("True");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteRating(int ContestID)
        {
            if (!IsMaster())
                return Content("False");
            System.Threading.Tasks.Task.Factory.StartNew(() => { Helpers.Ratings.RatingDelete(ContestID); });
            return Content("True");
        }

        [Authorize]
        public ActionResult VIP()
        {
            if (!IsMaster())
                return Message("您没有权限执行本操作！");
            var requests = (from vr in DbContext.VIPRequests
                            where vr.StatusAsInt == (int)DataModels.VIPRequestStatus.Pending
                            orderby vr.Time ascending
                            select vr).ToList();
            return View(requests);
        }

        [Authorize]
        [HttpPost]
        public ActionResult VIP(int id, string Reason, int status)
        {
            if (!IsMaster())
                return Content("Failed");
            var vr = DbContext.VIPRequests.Find(id);
            vr.StatusAsInt = status;
            vr.Reason = Reason;
            if (vr.Status == DataModels.VIPRequestStatus.Accepted)
                vr.User.Role = DataModels.UserRole.VIP;
            DbContext.SaveChanges();
            return Content("OK");
        }

        [Authorize]
        public ActionResult Problem()
        {
            if (!IsMaster())
                return Message("您没有权限执行本操作！");
            var problems = (from p in DbContext.Problems
                            where !p.Official
                            orderby p.Reviews.Where(x => x.LevelAsInt == (int)Tyvj.DataModels.ReviewLevel.Good).Count() descending,
                             p.Reviews.Where(x => x.LevelAsInt == (int)Tyvj.DataModels.ReviewLevel.Medium).Count() descending,
                             p.Reviews.Where(x => x.LevelAsInt == (int)Tyvj.DataModels.ReviewLevel.Bad).Count() descending
                            select p).ToList();
            return View(problems);
        }
    }
}