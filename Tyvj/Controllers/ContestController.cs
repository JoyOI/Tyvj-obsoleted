using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tyvj.DataModels;
using Tyvj.ViewModels;

namespace Tyvj.Controllers
{
    public class ContestController : BaseController
    {
        public static List<Contest> ContestListCache = new List<Contest>();

        // GET: Contest
        public ActionResult Index()
        {
            var _contests = (from c in ContestListCache
                                              where DateTime.Now >= c.Begin && DateTime.Now < c.End
                                              orderby c.End ascending
                                              select c).ToList();
            var contests = new List<vContest>();
            foreach (var c in _contests)
                contests.Add(new vContest(c));
            return View(contests);
        }

        public static void RefreshContestListCache()
        {
            System.Threading.Tasks.Task.Factory.StartNew(() => {
                var DbContext = new DB();
                ContestListCache = (from c in DbContext.Contests
                                    orderby c.ID descending
                                    select c).ToList();
            });
        }

        [HttpGet]
        public ActionResult GetContests(int? Page, string Title, int? Format)
        { 
            if(Title == null)Title = "";
            if(Page == null)Page = 0;
            IEnumerable<Contest> _contests = (from c in ContestListCache
                                where c.Title.Contains(Title)
                                && !(DateTime.Now >= c.Begin && DateTime.Now <c.End)
                                && c.ContestProblems.Count > 0
                                select c);
            if (Format != null)
                _contests = _contests.Where(x => x.FormatAsInt == Format.Value).OrderByDescending(x=>x.End).Skip(10 * Page.Value).Take(10).ToList();
            else _contests = _contests.OrderByDescending(x => x.End).Skip(10 * Page.Value).Take(10).ToList();
            var contests = new List<vContest>();
            foreach (var c in _contests)
                contests.Add(new vContest(c));
            return Json(contests, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Show(int id)
        { 
            var contest = DbContext.Contests.Find(id);
            return View(contest);
        }

        public ActionResult Standings(int id)
        {
            var contest = DbContext.Contests.Find(id);
            var user = ViewBag.CurrentUser == null ? new User() : (User)ViewBag.CurrentUser;
            if (!Helpers.Contest.UserInContest(user.ID,id))
            {
                switch (contest.JoinMethod)
                {
                    case ContestJoinMethod.Password:
                        return RedirectToAction("Register", "Contest", new { id = id });
                    case ContestJoinMethod.Appoint:
                        return Message("您未被邀请参加本场比赛！");
                    case ContestJoinMethod.Group:
                        return Message("本场比赛仅限" + contest.Group.Title + "团队内部参加！");
                }
            }
            if (contest.Format == ContestFormat.OI && DateTime.Now < contest.End && !IsMaster())
                return Message("目前不提供比赛排名显示。");
            ViewBag.AllowHack = false;
            if (User.Identity.IsAuthenticated)
            {
                if (contest.Format == ContestFormat.Codeforces && DateTime.Now >= contest.Begin && DateTime.Now < contest.End)
                    ViewBag.AllowHack = true;
            }
            return View(contest);
        }

        [HttpGet]
        public ActionResult GetStandings(int id)
        {
            var user = ViewBag.CurrentUser == null ? new User() : (User)ViewBag.CurrentUser;
            var contest = DbContext.Contests.Find(id);
            if (!Helpers.Contest.UserInContest(user.ID, id))
            {
                switch (contest.JoinMethod)
                {
                    case ContestJoinMethod.Password:
                        return RedirectToAction("Register", "Contest", new { id = id });
                    case ContestJoinMethod.Appoint:
                        return Message("您未被邀请参加本场比赛！");
                    case ContestJoinMethod.Group:
                        return Message("本场比赛仅限" + contest.Group.Title + "团队内部参加！");
                }
            }
                
            if (contest.Format == ContestFormat.OI && DateTime.Now < contest.End && !IsMaster())
                return Json(null, JsonRequestBehavior.AllowGet);
            var standings = Helpers.Standings.Build(id);
            return Json(standings, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult Register(int id)
        {
            var contest = DbContext.Contests.Find(id);
            return View(contest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Register(int id, string Password)
        {
            var contest = DbContext.Contests.Find(id);
            if (string.IsNullOrEmpty(contest.Password) || contest.Password == Password)
            {
                DbContext.ContestRegisters.Add(new ContestRegister
                {
                    ContestID = id,
                    UserID = ViewBag.CurrentUser.ID
                });
                DbContext.SaveChanges();
                return RedirectToAction("Show", "Contest", new { id = id });
            }
            else
            {
                return Message("参赛密码不正确！");
            }
        }

        public ActionResult Statistics(int id)
        {
            var contest = DbContext.Contests.Find(id);
            var user = ViewBag.CurrentUser == null ? new User() : (User)ViewBag.CurrentUser;
            if (!Helpers.Contest.UserInContest(user.ID, id))
            {
                switch (contest.JoinMethod)
                {
                    case ContestJoinMethod.Password:
                        return RedirectToAction("Register", "Contest", new { id = id });
                    case ContestJoinMethod.Appoint:
                        return Message("您未被邀请参加本场比赛！");
                    case ContestJoinMethod.Group:
                        return Message("本场比赛仅限" + contest.Group.Title + "团队内部参加！");
                }
            }
            var statistics = new int[contest.ContestProblems.Count, 9];
            var i = 0;
            foreach (var p in contest.ContestProblems.OrderBy(x => x.Point))
            {
                var statuses = Helpers.Contest.GetStatuses(p.ProblemID, id).ToList();
                if (contest.Format == ContestFormat.OI)
                {
                    var user_ids = (from s in statuses
                                    select s.UserID).Distinct();
                    foreach (var uid in user_ids)
                    {
                        var last_status = statuses.Where(x => x.UserID == uid).LastOrDefault();
                        if (last_status != null)
                        {
                            if (last_status.ResultAsInt < 8)
                                statistics[i, last_status.ResultAsInt]++;
                            else
                                statistics[i, 8]++;
                        }
                    }
                }
                else
                {
                    statistics[i, 0] = statuses.Where(x => x.Result == JudgeResult.Accepted).Count();
                    statistics[i, 1] = statuses.Where(x => x.Result == JudgeResult.PresentationError).Count();
                    statistics[i, 2] = statuses.Where(x => x.Result == JudgeResult.WrongAnswer).Count();
                    statistics[i, 3] = statuses.Where(x => x.Result == JudgeResult.OutputLimitExceeded).Count();
                    statistics[i, 4] = statuses.Where(x => x.Result == JudgeResult.TimeLimitExceeded).Count();
                    statistics[i, 5] = statuses.Where(x => x.Result == JudgeResult.MemoryLimitExceeded).Count();
                    statistics[i, 6] = statuses.Where(x => x.Result == JudgeResult.RuntimeError).Count();
                    statistics[i, 7] = statuses.Where(x => x.Result == JudgeResult.CompileError).Count();
                    statistics[i, 8] = statuses.Where(x => x.Result == JudgeResult.Hacked).Count();
                }
                i++;
            }
            ViewBag.Statistics = statistics;
            return View(contest);
        }

        [Authorize]
        public ActionResult Edit(int id)
        {
            var contest = DbContext.Contests.Find(id);
            if (!IsMaster() && CurrentUser.ID != contest.UserID)
                return Message("您无权执行本操作");
            ViewBag.Groups = (from g in DbContext.Groups
                          where g.UserID == contest.UserID
                          select g).ToList();
            return View(contest);
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult Edit(int id, string Title, string Description, DateTime Begin, DateTime End, int Format, bool? Official)
        {
            var contest = DbContext.Contests.Find(id);
            if (!IsMaster() && CurrentUser.ID != contest.UserID)
                return Message("您无权执行本操作");
            if (Title.Length == 0) return Message("请输入比赛名称");
            contest.Title = Title;
            contest.Description = Description;
            contest.Begin = Begin;
            contest.End = End;
            contest.FormatAsInt = Format;
            if (CurrentUser.Role >= UserRole.Master)
            {
                if (!contest.Official && Official.Value)
                {
                    contest.User.Coins += 50;
                }
                contest.Official = Official.Value;
            }
            DbContext.SaveChanges();
            RefreshContestListCache();
            return RedirectToAction("Edit", "Contest", new { id = id });
        }

        [Authorize]
        public ActionResult Problem(int id)
        {
            var contest = DbContext.Contests.Find(id);
            if (!IsMaster() && CurrentUser.ID != contest.UserID)
                return Message("您无权执行本操作");
            return View(contest);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult AddProblem(int id, string Number, int PID, int Point)
        {
            var contest = DbContext.Contests.Find(id);
            if (!IsMaster() && CurrentUser.ID != contest.UserID)
                return Message("您无权执行本操作");
            if ((from cp in DbContext.ContestProblems where cp.ContestID == id && cp.ProblemID == PID select cp).Count() > 0)
            {
                return Message("请不要重复添加同一道题目");
            }
            var Problem = DbContext.Problems.Find(PID);
            if (Problem.VIP && ViewBag.CurrentUser.Role < UserRole.Master && ViewBag.CurrentUser.ID != Problem.ID)
            {
                return Message("您没有权限使用这道题");
            }
            if (Problem == null)
                return Message("没有找到这道题目");
            if (!IsMaster() && Problem.Hide && Problem.UserID != CurrentUser.ID)
                return Message("您没有权限使用这道题");
            if (contest.Format == ContestFormat.Codeforces && (string.IsNullOrEmpty(Problem.RangeValidator)||string.IsNullOrEmpty(Problem.StandardProgram)))
            {
                return Message("这道题不适合作为Codeforces赛制题目");
            }
            DbContext.ContestProblems.Add(new ContestProblem 
            { 
                ContestID = id,
                Number = Number,
                ProblemID = PID,
                Point = Point
            });
            DbContext.SaveChanges();
            RefreshContestListCache();
            return RedirectToAction("Problem", "Contest", new { id = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult DeleteProblem(int id, int CPID)
        {
            var contest = DbContext.Contests.Find(id);
            if (!IsMaster() && CurrentUser.ID != contest.UserID)
                return Message("您无权执行本操作");
            var cp = DbContext.ContestProblems.Find(CPID);
            DbContext.ContestProblems.Remove(cp);
            DbContext.SaveChanges();
            RefreshContestListCache();
            return RedirectToAction("Problem", "Contest", new { id = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Create()
        {
            var contest = new Contest 
            { 
                Begin = Convert.ToDateTime("2099-1-1 12:00"),
                End = Convert.ToDateTime("2099-1-1 18:00"),
                Title = CurrentUser.Username + "创建的比赛",
                Description = "请在此处填写比赛描述",
                Format = ContestFormat.OI,
                Password = "",
                UserID = CurrentUser.ID,
                JoinMethod = ContestJoinMethod.Everyone,
                GroupID = null
            };
            DbContext.Contests.Add(contest);
            DbContext.SaveChanges();
            RefreshContestListCache();
            return RedirectToAction("Edit", "Contest", new { id=contest.ID});
        }

        public ActionResult AddCompetitor(int id, string Username)
        {
            var contest = DbContext.Contests.Find(id);
            if (!IsMaster() && CurrentUser.ID != contest.UserID)
                return Content("No access");
            var user = (from u in DbContext.Users
                        where u.Username == Username
                        select u).FirstOrDefault();
            if (user == null)
                return Content("User not found");
            contest.JoinMethod = ContestJoinMethod.Appoint;
            var cr = new ContestRegister
            {
                UserID = user.ID,
                ContestID = contest.ID
            };
            DbContext.ContestRegisters.Add(cr);
            DbContext.SaveChanges();
            return Content(cr.ID.ToString());
        }

        public ActionResult DeleteCompetitor(int id)
        {
            var competitor = DbContext.ContestRegisters.Find(id);
            if (!IsMaster() && CurrentUser.ID != competitor.Contest.UserID)
                return Content("您无权执行本操作！");
            if (competitor.Contest.JoinMethod != ContestJoinMethod.Appoint)
                return Content("您无权执行本操作！");
            DbContext.ContestRegisters.Remove(competitor);
            DbContext.SaveChanges();
            return Content("OK");
        }

        public ActionResult SetJoinMode(int id, int JoinMode, int? GroupID, string Password)
        {
            var contest = DbContext.Contests.Find(id);
            if (!IsMaster() && CurrentUser.ID != contest.UserID)
                return Content("您无权执行本操作！");
            contest.JoinMethod = (ContestJoinMethod)JoinMode;
            switch (contest.JoinMethod)
            {
                case ContestJoinMethod.Everyone:
                    break;
                case ContestJoinMethod.Password:
                    if (string.IsNullOrEmpty(Password))
                        return Content("密码不能为空！");
                    contest.Password = Password;
                    break;
                case ContestJoinMethod.Group:
                    if (!GroupID.HasValue)
                        return Content("请选择一个团队");
                    var group = DbContext.Groups.Find(GroupID.Value);
                    if (group == null || group.UserID != CurrentUser.ID)
                        return Content("没有该团队或您不是团队管理员。");
                    contest.GroupID = GroupID.Value;
                    break;
            }
            DbContext.SaveChanges();
            return Content("参赛方式保存成功！");
        }
    }
}