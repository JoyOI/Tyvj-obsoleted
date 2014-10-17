using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Tyvj.DataModels;
using Tyvj.ViewModels;

namespace Tyvj.Controllers
{
    public class UserController : BaseController
    {
        // GET: User
        public ActionResult Index(int id)
        {
            var user = DbContext.Users.Find(id);
            var ac_list = (from s in DbContext.Statuses
                           where s.UserID == id
                           && s.Problem.Hide == false
                           && s.ResultAsInt == (int)JudgeResult.Accepted
                           orderby s.ProblemID ascending
                           select s.ProblemID).Distinct().ToList();
            ac_list.Sort((a, b) => { return a - b; });
            ViewBag.AcceptedList = ac_list;
            return View(user);
        }

        public ActionResult Login()
        {
            if (User.Identity.IsAuthenticated == true)
            {
                return Redirect("/");
            }
            if (User.Identity.IsAuthenticated)
                return Redirect("/");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(vLogin model)
        {
            if (User.Identity.IsAuthenticated == true)
            {
                return Redirect("/");
            }
            var user = (from u in DbContext.Users
                        where u.Username == model.Username
                        select u).SingleOrDefault();
            if (user == null)
            {
                ViewBag.Info = "不存在这个用户！";
                return View();
            }

            //更新md5密码为sha1
            if (user.Password.Length == 32)
            {
                if (Helpers.Security.MD5(model.Password).ToUpper() == user.Password.ToUpper())
                {
                    user.Password = Helpers.Security.SHA1(model.Password);
                    DbContext.SaveChanges();
                }
            }

            //更新明文密码为sha1
            if (user.Password.Length < 16)
            {
                if (user.Password == model.Password)
                {
                    user.Password = Helpers.Security.SHA1(model.Password);
                    DbContext.SaveChanges();
                }
            }

            if (user.Password != Helpers.Security.SHA1(model.Password))
            {
                ViewBag.Info = "密码错误！";
                return View();
            }
            else
            {
                FormsAuthentication.SetAuthCookie(model.Username, model.Remember);
                if (Request.UrlReferrer == null)
                    return Redirect("/");
                else
                    return Redirect(Request.UrlReferrer.ToString());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            var user = (from u in DbContext.Users
                        where u.Username == User.Identity.Name
                        select u).Single();
            DbContext.SaveChanges();
            FormsAuthentication.SignOut();
            return Redirect(Request.UrlReferrer.ToString());
        }

        public ActionResult Register()
        {
            if (User.Identity.IsAuthenticated == true)
            {
                return Redirect("/");
            }
            return View();
        }

        [Authorize]
        public ActionResult Settings(int id)
        {
            var user = DbContext.Users.Find(id);
            return View(user);
        }
    }
}