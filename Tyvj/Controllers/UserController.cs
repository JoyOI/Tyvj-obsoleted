using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Text.RegularExpressions;
using System.Net;
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
            if (Request.UrlReferrer == null)
            {
                ViewBag.Info = "不存在这个用户！";
                return View();
            }
            if (User.Identity.IsAuthenticated == true)
            {
                return Redirect("/");
            }
            User user;
            if(model.Username.IndexOf("@") > 0)
                user = (from u in DbContext.Users
                        where u.Email == model.Username
                        select u).SingleOrDefault();
            else
                user = (from u in DbContext.Users
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

            //更新提交/ac数据
            if (string.IsNullOrEmpty(user.SubmitList) || string.IsNullOrEmpty(user.AcceptedList) || true)
            {
                var sub = (from s in DbContext.Statuses
                           where s.UserID == user.ID
                           select s.ProblemID).Distinct().ToList();
                user.SubmitList = Helpers.AcList.ToString(sub);
                user.SubmitCount = (from s in DbContext.Statuses
                                    where s.UserID == user.ID
                                    select s).Count();
                var ac = (from s in DbContext.Statuses
                          where s.UserID == user.ID
                            && s.ResultAsInt == 0
                          select s.ProblemID).Distinct().ToList();
                user.AcceptedCount = ac.Count;
                user.AcceptedList = Helpers.AcList.ToString(ac);
                DbContext.SaveChanges();
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
                if (user.Role == UserRole.Temporary)
                    return Message("您已被封号，系统禁止您登录！");
                FormsAuthentication.SetAuthCookie(user.Username, model.Remember);
                user.LastLoginTime = DateTime.Now;
                DbContext.SaveChanges();
                Helpers.Gravatar.RefreshGravatar(user.ID);
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

        public ActionResult Contests(int id)
        {
            var user = DbContext.Users.Find(id);
            var contests = (from c in ContestController.ContestListCache
                            where c.UserID == id
                            orderby c.ID descending
                            select c).ToList();
            ViewBag.Contests = contests;
            return View(user);
        }

        [HttpPost]
        public ActionResult Register(string Email)
        {
            if ((from u in DbContext.Users where u.Email == Email select u).Count() > 0)
                return Message("这个电子邮箱已经被注册过了，请返回更换电子邮箱后重新尝试注册！");
            if (!Helpers.Regexes.Email.IsMatch(Email))
                return Message("您输入的电子邮箱不合法，请返回更换电子邮箱后重新尝试注册！");

            EmailVerification email_verification = (from ev in DbContext.EmailVerifications
                                                    where ev.Email == Email
                                                    && ev.TypeAsInt == (int)VerifyType.Register
                                                    select ev).SingleOrDefault();
            if (email_verification == null)
            {
                email_verification = new EmailVerification
                {
                    Email = Email,
                    Time = DateTime.Now.AddHours(2),
                    Token = Helpers.String.RandomString(16),
                    Type = VerifyType.Register
                };
                DbContext.EmailVerifications.Add(email_verification);
            }
            else
            {
                email_verification.Time = DateTime.Now.AddHours(2);
                email_verification.Token = Helpers.String.RandomString(16);
            }
            DbContext.SaveChanges();

            var Sitename = "Tyvj";
            var SiteAddress = Request.Url.ToString().Replace(Request.RawUrl.ToString(), "");
            string strEmail = "<!DOCTYPE HTML><html><head><meta charset=\"UTF-8\"/><title>[Title]</title><style>p{margin:5px 0px}a{color:#1D76C7;text-decoration:none}.body{margin:0px;color:#333333;font-size:14px;font-family:Tahoma,'Segoe UI',Verdana,微软雅黑,'Microsoft YaHei',宋体;padding:30px;background-color:#F2F2F2}.container{box-shadow:rgba(0,0,0,0.3)0px 0px 15px;border-top-left-radius:5px;border-top-right-radius:5px;border-bottom-left-radius:5px;border-bottom-right-radius:5px;background-color:#FFF}.header{color:#FFF;padding:10px;line-height:200%;font-size:15px;border-top-left-radius:5px;border-top-right-radius:5px;border-bottom-left-radius:0px;border-bottom-right-radius:0px;border-bottom-width:3px;border-bottom-style:solid;border-bottom-color:#85CAEB;background-color:#3AA9DE}.problem-body{padding:30px}.link{padding:5px 10px;border-left-width:10px;border-left-style:solid;border-left-color:#E2EFFA;margin:20px 20px 20px 0px}.footer{color:#444;padding:10px;font-size:12px;border-top-width:1px;border-top-style:solid;border-top-color:#DDD;border-top-left-radius:0px;border-top-right-radius:0px;border-bottom-left-radius:5px;border-bottom-right-radius:5px;background-color:#F4F4F4}</style></head><body><div class=\"body\"><div class=\"container\"><div class=\"header\">新用户激活 - "+Sitename+"</div><div class=\"body\"><p><strong>您好，欢迎您注册"+Sitename+"帐号，请根据下面的提示信息继续完成注册操作。</strong></p><p>请点击下面的链接完成帐号工作，激活成功后将会自动登录"+Sitename+"系统：</p><blockquote class=\"link\"><p><a href=\""+SiteAddress+"/Verify/Register/" + email_verification.ID + "/" + email_verification.Token + "\" target=\"_blank\">"+SiteAddress+"/Verify/Register/" + email_verification.ID + "/" + email_verification.Token + "</a></p></blockquote><p>如果这次操作不是您本人的行为，请忽略本条邮件。</p></div><div class=\"footer\"><p>这封邮件由<a href=\""+SiteAddress+"\"target=\"_blank\">"+Sitename+"</a>自动发送，请勿直接回复。</p></div></div></div></body></html>";

            Helpers.SMTP.Send(email_verification.Email, Sitename+" 用户注册邮箱验证", strEmail);
            return Message( "我们已经向" + email_verification.Email + "中发送了一封包含验证链接的电子邮件，请根据电子邮件中的提示进行下一步的注册。");
        }

        public ActionResult RegisterDetail()
        {
            if (Session["Email"] == null)
                return Message("非法访问");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RegisterDetail(vRegister model)
        {
            if (Session["Email"] == null)
                return Message("非法访问。");
            if (!Regex.IsMatch(model.Username, @"^\w+$") || Helpers.String.StringLen(model.Username) < 4 || Helpers.String.StringLen(model.Username) > 16)
                return Message("用户名不合法，用户名长度必须为4~16个字符，同时只允许使用英文字母、数字和下划线" );
            var user = (from u in DbContext.Users
                        where u.Username == model.Username
                        select u).SingleOrDefault();
            var email = Session["Email"].ToString();
            if (user != null) return Message("用户名已经存在，请返回更换用户名再试！" );
            DbContext.Users.Add(new User
            {
                Username = model.Username,
                Password = Helpers.Security.SHA1(model.Password),
                Email = email,
                LastLoginTime = DateTime.Now,
                RegisterTime = DateTime.Now,
                Role = UserRole.Member,
                Motto = "",
                CommonLanguage = Language.C,
                Gravatar = email,
                QQ = model.QQ,
                School = model.School,
                AcceptedList = "",
                SubmitList = "",
                Sex = Sex.Male,
                AcceptedCount = 0,
                SubmitCount = 0,
                Avatar = new byte[0],
                Address = "",
                Name = "",
                Coins = 0
            });
            DbContext.SaveChanges();
            var email_verification = (from ev in DbContext.EmailVerifications
                                      where ev.Email == email
                                      select ev).Single();
            DbContext.EmailVerifications.Remove(email_verification);
            DbContext.SaveChanges();
            return Message("注册成功，您可以通过右上方登录按钮进行登录操作");
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(int id, string OldPassword, string NewPassword, string ConfirmPassword)
        {
            if (CurrentUser.Role < UserRole.Master && CurrentUser.ID != id)
                return Content("您没有权限执行本操作");
            if (NewPassword != ConfirmPassword)
                return Content("两次密码输入不一致");
            var user = DbContext.Users.Find(id);
            if (CurrentUser.Role < UserRole.Master)
            {
                if (Helpers.Security.SHA1(OldPassword) != CurrentUser.Password)
                    return Content("旧密码不正确");
            }
            user.Password = Helpers.Security.SHA1(NewPassword);
            DbContext.SaveChanges();
            return Content("密码修改成功");
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeProfile(int id, string QQ, string School,string Name, string Address, int Sex, int CommonLanguage, string Motto, string Phone)
        {
            if (CurrentUser.Role < UserRole.Master && CurrentUser.ID != id)
                return Content("您没有权限执行本操作");
            var user = DbContext.Users.Find(id);
            user.QQ = QQ;
            user.School = School;
            user.SexAsInt = Sex;
            user.CommonLanguageAsInt = CommonLanguage;
            user.Motto = Motto;
            user.Address = Address;
            user.Name = Name;
            user.Phone = Phone;
            DbContext.SaveChanges();
            return Content("个人资料修改成功");
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeRole(int id, int Role)
        {
            if (CurrentUser.Role < UserRole.Root)
                return Content("您无权执行本操作");
            var user = DbContext.Users.Find(id);
            user.RoleAsInt = Role;
            DbContext.SaveChanges();
            return Content("用户角色修改成功");
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeAvatar(int id, int AvatarMode, string Gravatar)
        {
            if (CurrentUser.Role < UserRole.Master && CurrentUser.ID != id)
                return Message("您没有权限执行本操作!");
            if (AvatarMode == 1)
            {
                if(!Helpers.Regexes.Email.IsMatch(Gravatar))
                    return Message("Gravatar邮箱地址不合法!");
                var user = DbContext.Users.Find(id);
                user.Gravatar = Gravatar;
                var wc = new WebClient();
                //user.Avatar = wc.DownloadData(Helpers.Gravatar.GetAvatarURL(user.Gravatar, 180));
                DbContext.SaveChanges();
                return RedirectToAction("Settings", "User", new { id = id });
            }
            else
            {
                var user = DbContext.Users.Find(id);
                var file = Request.Files[0];
                if (file.ContentLength > 256 * 1024)
                    return Message("您上传的头像文件大小为" + file.ContentLength / 1024 + "KB，超出系统规定的256KB，请返回重试！");
                var timestamp = Helpers.String.ToTimeStamp(DateTime.Now);
                var filename = timestamp + ".tmp";
                var dir = Server.MapPath("~") + @"\Temp\";
                file.SaveAs(dir + filename);
                user.Avatar = System.IO.File.ReadAllBytes(dir+filename);
                System.IO.File.Delete(dir + filename);
                user.Gravatar = null;
                DbContext.SaveChanges();
                return RedirectToAction("Settings", "User", new { id = id });
            }
        }

        [HttpGet]
        [Authorize]
        public ActionResult DailySign()
        {
            int uid = ViewBag.CurrentUser.ID;
            var today = DateTime.Now.Date;
            var cnt = (from ds in DbContext.DailySigns
                       where ds.UserID == uid
                       && ds.Time >= today
                       select ds).Count();
            if (cnt == 0)
            {
                DbContext.DailySigns.Add(new DataModels.DailySign
                {
                    UserID = uid,
                    Time = DateTime.Now
                });
                var user = DbContext.Users.Find(uid);
                user.Coins += 10;
                DbContext.SaveChanges();
                return Message("签到成功，您获得了 10 枚金币！");
            }
            else
            {
                return Message("您今天已经签到，请明天再来！");
            }
        }

        [Authorize]
        public ActionResult Quest()
        {
            var today = DateTime.Now.Date;
            var cnt = (from q in DbContext.Quests
                       where q.UserID == CurrentUser.ID
                       && q.Time >= today
                       select q).Count();
            if (cnt == 0)
            {
                var aclist = Helpers.AcList.GetList(CurrentUser.AcceptedList);
                var problem = (from p in DbContext.Problems
                               where p.AcceptedCount > 0
                               && !aclist.Contains(p.ID)
                               && !p.Hide
                               && !p.VIP
                               orderby Guid.NewGuid()
                               select p).FirstOrDefault();
                if(problem == null)
                    problem = (from p in DbContext.Problems
                               orderby Guid.NewGuid()
                               select p).FirstOrDefault();
                DbContext.Quests.Add(new Quest
                {
                    ProblemID = problem.ID,
                    UserID = CurrentUser.ID,
                    Status = QuestStatus.Pending,
                    Time = DateTime.Now
                });
                DbContext.SaveChanges();
            }
            var quest = (from q in DbContext.Quests
                         where q.UserID == CurrentUser.ID
                         && q.Time >= today
                         select q).FirstOrDefault();
            var check = (from s in DbContext.Statuses
                         where s.Time >= today
                         && s.ProblemID == quest.ProblemID
                         && s.UserID == quest.UserID
                         && s.ResultAsInt == (int)JudgeResult.Accepted
                         select s).Count() > 0;
            ViewBag.Check = check;
            ViewBag.Quest = quest;
            return View(CurrentUser);
        }

        [HttpGet]
        [Authorize]
        public ActionResult QuestFinish()
        {
            var today = DateTime.Now.Date;
            var quest = (from q in DbContext.Quests
                         where q.UserID == CurrentUser.ID
                         && q.Time >= today
                         select q).FirstOrDefault();
            var check = (from s in DbContext.Statuses
                         where s.Time >= today
                         && s.ProblemID == quest.ProblemID
                         && s.UserID == quest.UserID
                         && s.ResultAsInt == (int)JudgeResult.Accepted
                         select s).Count() > 0;
            if (check)
            {
                quest.Status = QuestStatus.Finished;
                var user = DbContext.Users.Find(CurrentUser.ID);
                user.Coins += 30;
                DbContext.SaveChanges();
                return Message("恭喜您完成了今日的日常任务，获得了30枚金币！");
            }
            else
            {
                return Message("你还未完成今日任务！");
            }
        }

        [Authorize]
        public ActionResult VIP()
        {
            ViewBag.CheckUserGroup = CurrentUser.Role == UserRole.Member;
            ViewBag.CheckQQ = CurrentUser.QQ != null && CurrentUser.QQ.Length >= 5;
            ViewBag.CheckAddress = CurrentUser.Address != null && CurrentUser.Address.Length >= 3;
            ViewBag.CheckSchool = CurrentUser.School != null && CurrentUser.School.Length >= 2;
            ViewBag.CheckName = CurrentUser.Name != null && CurrentUser.Name.Length >= 2;
            ViewBag.CheckPhone = CurrentUser.Phone != null && CurrentUser.Phone.Length >= 11;
            ViewBag.AllowRequest = false;
            var cnt = (from vr in DbContext.VIPRequests
                       where vr.UserID == CurrentUser.ID
                       && vr.StatusAsInt == (int)VIPRequestStatus.Pending
                       select vr).Count();
            ViewBag.LastRequest = (from vr in DbContext.VIPRequests
                                   where vr.UserID == CurrentUser.ID
                                   orderby vr.Time descending
                                   select vr).FirstOrDefault();
            if (ViewBag.CheckUserGroup && ViewBag.CheckQQ && ViewBag.CheckAddress && ViewBag.CheckSchool && ViewBag.CheckName && cnt == 0)
                ViewBag.AllowRequest = true;
            return View(CurrentUser);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult VIP(string Content)
        {
            ViewBag.CheckUserGroup = CurrentUser.Role == UserRole.Member;
            ViewBag.CheckQQ = CurrentUser.QQ.Length >= 5;
            ViewBag.CheckAddress = CurrentUser.Address.Length >= 3;
            ViewBag.CheckSchool = CurrentUser.School.Length >= 2;
            ViewBag.CheckName = CurrentUser.Name.Length >= 2;
            ViewBag.CheckPhone = CurrentUser.Phone.Length >= 11;
            ViewBag.AllowRequest = false;
            var cnt = (from vr in DbContext.VIPRequests
                       where vr.UserID == CurrentUser.ID
                       && vr.StatusAsInt == (int)VIPRequestStatus.Pending
                       select vr).Count();
            ViewBag.LastRequest = (from vr in DbContext.VIPRequests
                                   where vr.UserID == CurrentUser.ID
                                   orderby vr.Time descending
                                   select vr).FirstOrDefault();
            if (ViewBag.CheckUserGroup && ViewBag.CheckQQ && ViewBag.CheckAddress && ViewBag.CheckSchool && ViewBag.CheckName && ViewBag.CheckPhone && cnt == 0)
                ViewBag.AllowRequest = true;
            if (ViewBag.AllowRequest)
            {
                DbContext.VIPRequests.Add(new VIPRequest
                {
                    Status = VIPRequestStatus.Pending,
                    Time = DateTime.Now,
                    UserID = CurrentUser.ID,
                    Reason = "",
                    Content = Content
                });
                DbContext.SaveChanges();
                return Message("您的申请已提交，请耐心等待审核。");
            }
            else
            {
                return Message("请勿重复提交申请！");
            }
        }

        public ActionResult Forgot()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Forgot(string Email)
        {
            var user = (from u in DbContext.Users
                        where u.Email == Email
                        select u).SingleOrDefault();
            if (user == null) return Message("没有找到这个邮箱对应的帐号！");
            EmailVerification email_verification = (from ev in DbContext.EmailVerifications
                                                    where ev.Email == Email
                                                    && ev.TypeAsInt == (int)VerifyType.Forgot
                                                    select ev).SingleOrDefault();
            if (email_verification == null)
            {
                email_verification = new EmailVerification
                {
                    Email = Email,
                    Time = DateTime.Now.AddHours(2),
                    Token = Helpers.String.RandomString(16),
                    Type = VerifyType.Forgot
                };
                DbContext.EmailVerifications.Add(email_verification);
            }
            else
            {
                email_verification.Time = DateTime.Now.AddHours(2);
                email_verification.Token = Helpers.String.RandomString(16);
            }
            DbContext.SaveChanges();
            var Sitename = "Tyvj";
            var SiteAddress = Request.Url.ToString().Replace(Request.RawUrl.ToString(), "");
            string strEmail = "<!DOCTYPE HTML><html><head><meta charset=\"UTF-8\"/><title>[Title]</title><style>p{margin:5px 0px}a{color:#1D76C7;text-decoration:none}.body{margin:0px;color:#333333;font-size:14px;font-family:Tahoma,'Segoe UI',Verdana,微软雅黑,'Microsoft YaHei',宋体;padding:30px;background-color:#F2F2F2}.container{box-shadow:rgba(0,0,0,0.3)0px 0px 15px;border-top-left-radius:5px;border-top-right-radius:5px;border-bottom-left-radius:5px;border-bottom-right-radius:5px;background-color:#FFF}.header{color:#FFF;padding:10px;line-height:200%;font-size:15px;border-top-left-radius:5px;border-top-right-radius:5px;border-bottom-left-radius:0px;border-bottom-right-radius:0px;border-bottom-width:3px;border-bottom-style:solid;border-bottom-color:#85CAEB;background-color:#3AA9DE}.problem-body{padding:30px}.link{padding:5px 10px;border-left-width:10px;border-left-style:solid;border-left-color:#E2EFFA;margin:20px 20px 20px 0px}.footer{color:#444;padding:10px;font-size:12px;border-top-width:1px;border-top-style:solid;border-top-color:#DDD;border-top-left-radius:0px;border-top-right-radius:0px;border-bottom-left-radius:5px;border-bottom-right-radius:5px;background-color:#F4F4F4}</style></head><body><div class=\"body\"><div class=\"container\"><div class=\"header\">找回密码 - " + Sitename + "</div><div class=\"body\"><p><strong>您好，我们收到了您找回密码的请求，请根据下面的提示信息继续完成注册操作。</strong></p><p>请点击下面的链接完成找回工作，当您设置新的密码后将会自动登录" + Sitename + "系统：</p><blockquote class=\"link\"><p><a href=\"" + SiteAddress + "/Verify/Forgot/" + email_verification.ID + "/" + email_verification.Token + "\" target=\"_blank\">" + SiteAddress + "/Verify/Forgot/" + email_verification.ID + "/" + email_verification.Token + "</a></p></blockquote><p>如果这次操作不是您本人的行为，请忽略本条邮件。</p></div><div class=\"footer\"><p>这封邮件由<a href=\"" + SiteAddress + "\"target=\"_blank\">" + Sitename + "</a>自动发送，请勿直接回复。</p></div></div></div></body></html>";
            Helpers.SMTP.Send(email_verification.Email, Sitename + " 找回密码邮箱验证", strEmail);
            return Message("我们已经向您的邮箱中邮递了一封带有验证链接的信件，请根据邮件链接中的提示继续操作！");
        }

        [Authorize]
        public ActionResult Coin()
        {
            ViewBag.CoinLogs = (from cl in DbContext.CoinLogs
                                where cl.ReceiverUserID == CurrentUser.ID || cl.GiverUserID == CurrentUser.ID
                                orderby cl.Time descending
                                select cl).Take(20).ToList();
            return View(CurrentUser);
        }

        [Authorize]
        [HttpPost]
        public ActionResult Coin(int Count, string Username)
        {
            if (Count <= 0)
                return Message("金币数量不合法");
            if (CurrentUser.Coins >= Count)
            {
                var user = (from u in DbContext.Users
                            where u.Username == Username
                            select u).SingleOrDefault();
                if (user == null)
                    return Message("没有找到指定的用户！");
                CurrentUser.Coins -= Count;
                user.Coins += Count;
                DbContext.CoinLogs.Add(new CoinLog {
                    GiverUserID = CurrentUser.ID,
                    ReceiverUserID = user.ID,
                    Count = Count,
                    Time = DateTime.Now
                });
                DbContext.SaveChanges();
                return Message("转账成功！");
            }
            else
            {
                return Message("您的金币余额不足！");
            }
        }
    }
}