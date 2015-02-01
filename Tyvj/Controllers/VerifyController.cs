using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Tyvj.Controllers
{
    public class VerifyController : BaseController
    {
        // GET: Verify
        public ActionResult Register(int id, string token)
        {
            var email_verification = DbContext.EmailVerifications.Find(id);
            if (email_verification == null || email_verification.Type == DataModels.VerifyType.Forgot)
                return Message("对不起，您的验证链接不合法，无法继续进行注册");
            if (DateTime.Now > email_verification.Time)
                return Message("对不起，您的验证信已经过期，请重新注册以验证电子邮箱");
            if (email_verification.Token != token)
                return Message("对不起，您的验证码不正确，请返回邮箱重新打开验证链接或重新注册！");
            Session["Email"] = email_verification.Email;
            return RedirectToAction("RegisterDetail", "User", null);
        }

        public ActionResult Forgot(string id, string token)
        {
            var email_verification = DbContext.EmailVerifications.Find(id);
            if (email_verification == null || email_verification.Type == DataModels.VerifyType.Register)
                return Message("对不起，您的验证链接不合法，无法继续进行密码找回");
            if (DateTime.Now > email_verification.Time)
                return Message("对不起，您的验证信已经过期，请重新注册以验证电子邮箱");
            if (email_verification.Token != token)
                return Message("对不起，您的验证码不正确，请返回邮箱重新打开验证链接或重新申请找回！");
            Session["ForgotEmail"] = email_verification.Email;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(string Password, string Repeat)
        {
            if(Password != Repeat)return Message("两次密码输入不一致！");
            if (Session["ForgotEmail"] == null) return Message("未知错误！");
            var email = Session["ForgotEmail"].ToString();
            var user = (from u in DbContext.Users
                        where u.Email == email
                        select u).SingleOrDefault();
            if(user == null) return Message("未知错误！");
            user.Password = Helpers.Security.SHA1(Password);
            DbContext.SaveChanges();
            Session["ForgotEmail"] = null;
            return Message("新密码设置成功，请使用新密码登录！");
        }
    }
}