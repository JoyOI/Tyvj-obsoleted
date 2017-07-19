using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tyvj.DataModels;
using Tyvj.ViewModels;
using JoyOI.ManagementService.SDK;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Net.Http;
using System.Threading.Tasks;


namespace Tyvj.Controllers
{
    public class StatusController : BaseController
    {
        // GET: Status
        public ActionResult Index(int? uid, int? cid, int?pid)
        {
            ViewBag.DestUsername = null;
            ViewBag.DestContestID = null;
            ViewBag.DestProblemID = null;
            if (uid != null)
            {
                ViewBag.DestUsername = DbContext.Users.Find(uid).Username;
            }
            if (cid != null)
            {
                ViewBag.DestContestID = cid.Value;
            }
            if (pid != null)
            {
                ViewBag.DestProblemID = pid.Value;
            }
            return View();
        }

        [HttpGet]
        public ActionResult GetStatuses(int? Page, string Username, int? Result, int? ContestID, int? ProblemID)
        {
            var _statuses = from s in DbContext.Statuses select s;
            if (!string.IsNullOrEmpty(Username))
            {
                var destuser = (from u in DbContext.Users
                                where u.Username == Username
                                select u).FirstOrDefault();
                var uid = 0;
                if (destuser != null) uid = destuser.ID;
                _statuses = _statuses.Where(x => x.User.ID == uid);
            }
            if (Result != null)
            {
                _statuses = _statuses.Where(x => x.ResultAsInt == Result.Value);
            }
            if (ContestID != null)
                _statuses = _statuses.Where(x => x.ContestID == ContestID);
            if (ProblemID != null)
                _statuses = _statuses.Where(x => x.ProblemID == ProblemID);
            _statuses = _statuses.OrderByDescending(x => x.ID);
            if (Result.HasValue && !IsMaster())
            {
                if (User.Identity.IsAuthenticated)
                    _statuses.Where(x => x.Contest.FormatAsInt != (int)ContestFormat.OI || x.Contest.End <= DateTime.Now || x.Contest.UserID == CurrentUser.ID).ToList();
                else
                    _statuses.Where(x => x.Contest.FormatAsInt != (int)ContestFormat.OI || x.Contest.End <= DateTime.Now).ToList();
            }
            _statuses = _statuses.Skip(50 * Page.Value).Take(50);
            var statuses = new List<vStatus>();
            foreach (var status in _statuses.ToList())
            {
                var user = ViewBag.CurrentUser == null ? new User() : (User)ViewBag.CurrentUser;
                var contest = status.Contest;
                if (contest == null || DateTime.Now >= contest.End || user.Role >= UserRole.Master || contest.UserID == user.ID)
                {
                    statuses.Add(new vStatus(status));
                    continue;
                }
                if (contest.Format == ContestFormat.ACM)
                {
                    status.JudgeTasks = new List<JudgeTask>
                    { 
                        new JudgeTask
                        {
                            Result = status.Result,
                            MemoryUsage = status.JudgeTasks.Max(x=>x.MemoryUsage),
                            TimeUsage = status.JudgeTasks.Max(x=>x.TimeUsage),
                            Hint="比赛期间不提供详细信息",
                            StatusID = status.ID
                        }
                    };
                }
                else if (contest.Format == ContestFormat.OI)
                {
                    status.Result = JudgeResult.Hidden;
                    status.JudgeTasks = new List<JudgeTask>
                    { 
                        new JudgeTask
                        {
                            Result = JudgeResult.Hidden,
                            MemoryUsage = 0,
                            TimeUsage = 0,
                            Hint="比赛期间不提供详细信息",
                            StatusID = status.ID
                        }
                    };
                }
                else
                {
                    foreach (var jt in status.JudgeTasks)
                        jt.Hint = "比赛期间不提供详细信息";
                }
                statuses.Add(new vStatus(status));
            }
            GC.Collect();
            return Json(statuses, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public async Task<ActionResult> Create(int problem_id, string code, int language_id, int? contest_id)
        {
            var problem = DbContext.Problems.Find(problem_id);
            var user = (User)ViewBag.CurrentUser;
            if (problem == null || (problem.Hide && !IsMaster() && CurrentUser.ID != problem.ID && !contest_id.HasValue))
                return Content("Problem not existed");
            if(problem.VIP && user.Role < UserRole.VIP && problem.UserID != user.ID)
                return Content("Need VIP");
            Contest contest = new Contest();
            if (contest_id != null)
            {
                contest = DbContext.Contests.Find(contest_id.Value);
                if(DateTime.Now < contest.Begin || DateTime.Now >= contest.End)
                    return Content("Insufficient permissions");
                if(contest.ContestProblems.Where(x=>x.ProblemID == problem_id).Count() == 0)
                    return Content("Insufficient permissions");
                if (!Helpers.Contest.UserInContest(user.ID, contest_id.Value))
                    return Content("Insufficient permissions");
                ContestFormat[] SubmitAnyTime = { ContestFormat.ACM, ContestFormat.OI };
                if (DateTime.Now < contest.Begin && user.Role < UserRole.Master && contest.UserID != user.ID)
                {
                    return Content("Insufficient permissions");
                }
                if (contest.Format == ContestFormat.Codeforces && (from l in DbContext.Locks where l.ProblemID == problem_id && user.ID == l.UserID && l.ContestID == contest_id.Value select l).Count() > 0 && DateTime.Now < contest.End)
                {
                    return Content("Locked");
                }
            }

            var status = new Status
            {
                Code = code,
                LanguageAsInt = language_id,
                ProblemID = problem_id,
                UserID = user.ID,
                Time = DateTime.Now,
                Result = JudgeResult.Pending,
                ContestID = contest_id,
                Score = 0,
                TimeUsage = 0,
                MemoryUsage = 0
            };
            DbContext.Statuses.Add(status);
            problem.SubmitCount++;
            user.SubmitCount++;
            var submitlist = Helpers.AcList.GetList(user.SubmitList);
            submitlist.Add(problem.ID);
            user.SubmitList = Helpers.AcList.ToString(submitlist);
            var _testcase_ids = (from tc in problem.TestCases
                                where tc.Type != TestCaseType.Sample
                                orderby tc.Type ascending
                                select new { tc.ID, tc.InputBlobId }).ToList();
            var testcase_ids = _testcase_ids.Select(x => x.ID).ToList();
            if (contest_id != null)
            {
                if (DateTime.Now < contest.Begin || DateTime.Now >= contest.End || contest.Format == ContestFormat.ACM || contest.Format == ContestFormat.OI)
                {
                    testcase_ids = (from tc in problem.TestCases
                                    where tc.Type != TestCaseType.Sample
                                    orderby tc.Type ascending
                                    select tc.ID).ToList();
                }
                else if (contest.Format == ContestFormat.Codeforces)
                {
                    testcase_ids = (from tc in problem.TestCases
                                    where tc.Type == TestCaseType.Unilateralism
                                    orderby tc.Type ascending
                                    select tc.ID).ToList();
                    var statuses = (from s in DbContext.Statuses
                                    where s.ContestID == contest_id.Value
                                    && s.UserID == user.ID
                                    select s).ToList();
                    foreach (var s in statuses)
                    {
                        if (s.JudgeTasks == null) continue;
                        foreach (var jt in s.JudgeTasks)
                        {
                            testcase_ids.Add(jt.TestCaseID);
                        }
                    }
                    testcase_ids = testcase_ids.Distinct().ToList();
                }
                foreach (var id in testcase_ids)
                {
                    DbContext.JudgeTasks.Add(new JudgeTask
                    {
                        StatusID = status.ID,
                        TestCaseID = id,
                        Result = JudgeResult.Pending,
                        MemoryUsage = 0,
                        TimeUsage = 0,
                        Hint = ""
                    });
                }
                DbContext.SaveChanges();
                foreach (var jt in status.JudgeTasks)
                {
                    try
                    {
                        var group = SignalR.JudgeHub.GetNode();
                        if (group == null) return Content("No Online Judger");
                        SignalR.JudgeHub.context.Clients.Group(group).Judge(new CodeComb.Judge.Models.JudgeTask(jt));
                        SignalR.JudgeHub.ThreadBusy(group);
                    }
                    catch { }
                }
                SignalR.UserHub.context.Clients.Group("Status").onStatusCreated(new vStatus(status));//推送新状态
                if (contest.Format == ContestFormat.OI && DateTime.Now >= contest.Begin && DateTime.Now < contest.End)
                    return Content("OI");
            }
            else // 不是比赛任务
            {
                foreach (var id in _testcase_ids)
                {
                    DbContext.JudgeTasks.Add(new JudgeTask
                    {
                        StatusID = status.ID,
                        TestCaseID = id.ID,
                        Result = JudgeResult.Pending,
                        MemoryUsage = 0,
                        TimeUsage = 0,
                        Hint = ""
                    });
                }
                DbContext.SaveChanges();
                if (string.IsNullOrWhiteSpace(problem.SpecialJudge) && (status.Language == Language.C || status.Language == Language.Cxx || status.Language == Language.Pascal)) // 如果是c,c++,pascal则由JoyOI接管评测
                {
                    var sourceName = "Main."; // 拼接选手源代码文件名
                    if (status.Language == Language.C)
                        sourceName += "c";
                    else if (status.Language == Language.Cxx)
                        sourceName += "cpp";
                    else
                        sourceName += "pas";

                    var client = new ManagementServiceClient("https://mgmtsvc.1234.sh", @"D:\Tyvj\webapi-client.pfx", "123456");
                    var blobs = new List<BlobInfo>(20);
                    blobs.Add(new BlobInfo { Id = Guid.Parse("0083c7bd-7c14-1035-82ec-54eca0c82300"), Name = "Validator.out" }); // 将标准比较器放入文件集合中
                    var sourceBlobId = await client.PutBlobAsync(sourceName, System.Text.Encoding.UTF8.GetBytes(status.Code)); // 将选手文件上传至Management Service
                    blobs.Add(new BlobInfo { Id = sourceBlobId, Name = sourceName }); // 将选手代码放入文件集合中

                    // 准备测试用例
                    var caseIndex = 0;
                    foreach (var jt in status.JudgeTasks)
                    {
                        if (string.IsNullOrWhiteSpace(jt.TestCase.InputBlobId)) // 如果Management Service没有该测试用例则上传
                        {
                            var inputCaseBlobId = await client.PutBlobAsync("case_input_" + jt.TestCaseID + ".txt", System.Text.Encoding.UTF8.GetBytes(jt.TestCase.Input));
                            jt.TestCase.InputBlobId = inputCaseBlobId.ToString();
                        }
                        if (string.IsNullOrWhiteSpace(jt.TestCase.OutputBlobId)) // 如果Management Service没有该测试用例则上传
                        {
                            var outputCaseBlobId = await client.PutBlobAsync("case_output_" + jt.TestCaseID + ".txt", System.Text.Encoding.UTF8.GetBytes(jt.TestCase.Output));
                            jt.TestCase.OutputBlobId = outputCaseBlobId.ToString();
                        }
                        if (!blobs.Any(x => x.Id == Guid.Parse(jt.TestCase.InputBlobId)))
                        {
                            blobs.Add(new BlobInfo { Id = Guid.Parse(jt.TestCase.InputBlobId), Name = "input_" + caseIndex + ".txt" });
                            blobs.Add(new BlobInfo { Id = Guid.Parse(jt.TestCase.OutputBlobId), Name = "output_" + caseIndex + ".txt" });
                        }
                        else
                        {
                            continue;
                        }
                        ++caseIndex;
                    }
                    var stateMachineId = await client.PutStateMachineInstanceAsync("TyvjJudgeStateMachine", "http://tyvj.cn", blobs); // 创建StateMachine实例
                    status.StateMachineId = stateMachineId.ToString();
                    DbContext.SaveChanges();
                }
                else
                {
                    foreach (var jt in status.JudgeTasks)
                    {
                        try
                        {
                            var group = SignalR.JudgeHub.GetNode();
                            if (group == null) return Content("No Online Judger");
                            SignalR.JudgeHub.context.Clients.Group(group).Judge(new CodeComb.Judge.Models.JudgeTask(jt));
                            SignalR.JudgeHub.ThreadBusy(group);
                        }
                        catch { }
                    }
                }
                SignalR.UserHub.context.Clients.Group("Status").onStatusCreated(new vStatus(status));//推送新状态
            }

            SignalR.UserHub.context.Clients.Group("Status").onStatusChanged(new vStatus(status));
            GC.Collect();
            return Content(status.ID.ToString());
        }

        [HttpGet]
        public async Task<ActionResult> Benchmark(int problem_id = 1000, string code = "#include<iostream>\r\nusing namespace std;\r\nint main()\r\n{\r\nwhile(1);\r\nreturn 0;\r\n}", int language_id = 1, int? contest_id = null)
        {
            for (var i = 0; i < 100; i++)
            {
                var problem = DbContext.Problems.Find(problem_id);
                var user = (User)ViewBag.CurrentUser;
                if (problem == null || (problem.Hide && !IsMaster() && CurrentUser.ID != problem.ID && !contest_id.HasValue))
                    return Content("Problem not existed");
                if (problem.VIP && user.Role < UserRole.VIP && problem.UserID != user.ID)
                    return Content("Need VIP");
                Contest contest = new Contest();
                if (contest_id != null)
                {
                    contest = DbContext.Contests.Find(contest_id.Value);
                    if (DateTime.Now < contest.Begin || DateTime.Now >= contest.End)
                        return Content("Insufficient permissions");
                    if (contest.ContestProblems.Where(x => x.ProblemID == problem_id).Count() == 0)
                        return Content("Insufficient permissions");
                    if (!Helpers.Contest.UserInContest(user.ID, contest_id.Value))
                        return Content("Insufficient permissions");
                    ContestFormat[] SubmitAnyTime = { ContestFormat.ACM, ContestFormat.OI };
                    if (DateTime.Now < contest.Begin && user.Role < UserRole.Master && contest.UserID != user.ID)
                    {
                        return Content("Insufficient permissions");
                    }
                    if (contest.Format == ContestFormat.Codeforces && (from l in DbContext.Locks where l.ProblemID == problem_id && user.ID == l.UserID && l.ContestID == contest_id.Value select l).Count() > 0 && DateTime.Now < contest.End)
                    {
                        return Content("Locked");
                    }
                }

                var status = new Status
                {
                    Code = code,
                    LanguageAsInt = language_id,
                    ProblemID = problem_id,
                    UserID = user.ID,
                    Time = DateTime.Now,
                    Result = JudgeResult.Pending,
                    ContestID = contest_id,
                    Score = 0,
                    TimeUsage = 0,
                    MemoryUsage = 0
                };
                DbContext.Statuses.Add(status);
                problem.SubmitCount++;
                user.SubmitCount++;
                var submitlist = Helpers.AcList.GetList(user.SubmitList);
                submitlist.Add(problem.ID);
                user.SubmitList = Helpers.AcList.ToString(submitlist);
                var testcase_ids = (from tc in problem.TestCases
                                    where tc.Type != TestCaseType.Sample
                                    orderby tc.Type ascending
                                    select tc.ID).ToList();
                if (contest_id != null)
                {
                    if (DateTime.Now < contest.Begin || DateTime.Now >= contest.End || contest.Format == ContestFormat.ACM || contest.Format == ContestFormat.OI)
                    {
                        testcase_ids = (from tc in problem.TestCases
                                        where tc.Type != TestCaseType.Sample
                                        orderby tc.Type ascending
                                        select tc.ID).ToList();
                    }
                    else if (contest.Format == ContestFormat.Codeforces)
                    {
                        testcase_ids = (from tc in problem.TestCases
                                        where tc.Type == TestCaseType.Unilateralism
                                        orderby tc.Type ascending
                                        select tc.ID).ToList();
                        var statuses = (from s in DbContext.Statuses
                                        where s.ContestID == contest_id.Value
                                        && s.UserID == user.ID
                                        select s).ToList();
                        foreach (var s in statuses)
                        {
                            if (s.JudgeTasks == null) continue;
                            foreach (var jt in s.JudgeTasks)
                            {
                                testcase_ids.Add(jt.TestCaseID);
                            }
                        }
                        testcase_ids = testcase_ids.Distinct().ToList();
                    }
                    foreach (var id in testcase_ids)
                    {
                        DbContext.JudgeTasks.Add(new JudgeTask
                        {
                            StatusID = status.ID,
                            TestCaseID = id,
                            Result = JudgeResult.Pending,
                            MemoryUsage = 0,
                            TimeUsage = 0,
                            Hint = ""
                        });
                    }
                    DbContext.SaveChanges();
                    foreach (var jt in status.JudgeTasks)
                    {
                        try
                        {
                            var group = SignalR.JudgeHub.GetNode();
                            if (group == null) return Content("No Online Judger");
                            SignalR.JudgeHub.context.Clients.Group(group).Judge(new CodeComb.Judge.Models.JudgeTask(jt));
                            SignalR.JudgeHub.ThreadBusy(group);
                        }
                        catch { }
                    }
                    SignalR.UserHub.context.Clients.Group("Status").onStatusCreated(new vStatus(status));//推送新状态
                    if (contest.Format == ContestFormat.OI && DateTime.Now >= contest.Begin && DateTime.Now < contest.End)
                        return Content("OI");
                }
                else // 不是比赛任务
                {
                    foreach (var id in testcase_ids)
                    {
                        DbContext.JudgeTasks.Add(new JudgeTask
                        {
                            StatusID = status.ID,
                            TestCaseID = id,
                            Result = JudgeResult.Pending,
                            MemoryUsage = 0,
                            TimeUsage = 0,
                            Hint = ""
                        });
                    }
                    DbContext.SaveChanges();
                    if (string.IsNullOrWhiteSpace(problem.SpecialJudge) && (status.Language == Language.C || status.Language == Language.Cxx || status.Language == Language.Pascal)) // 如果是c,c++,pascal则由JoyOI接管评测
                    {
                        var sourceName = "Main."; // 拼接选手源代码文件名
                        if (status.Language == Language.C)
                            sourceName += "c";
                        else if (status.Language == Language.Cxx)
                            sourceName += "cpp";
                        else
                            sourceName += "pas";

                        var client = new ManagementServiceClient("https://mgmtsvc.1234.sh", @"D:\Tyvj\webapi-client.pfx", "123456");
                        var blobs = new List<BlobInfo>(20);
                        blobs.Add(new BlobInfo { Id = Guid.Parse("0083c7bd-7c14-1035-82ec-54eca0c82300"), Name = "Validator.out" }); // 将标准比较器放入文件集合中
                        var sourceBlobId = await client.PutBlobAsync(sourceName, System.Text.Encoding.UTF8.GetBytes(status.Code)); // 将选手文件上传至Management Service
                        blobs.Add(new BlobInfo { Id = sourceBlobId, Name = sourceName }); // 将选手代码放入文件集合中

                        // 准备测试用例
                        var caseIndex = 0;
                        foreach (var jt in status.JudgeTasks)
                        {
                            if (string.IsNullOrWhiteSpace(jt.TestCase.InputBlobId)) // 如果Management Service没有该测试用例则上传
                            {
                                var inputCaseBlobId = await client.PutBlobAsync("case_input_" + jt.TestCaseID + ".txt", System.Text.Encoding.UTF8.GetBytes(jt.TestCase.Input));
                                jt.TestCase.InputBlobId = inputCaseBlobId.ToString();
                            }
                            if (string.IsNullOrWhiteSpace(jt.TestCase.OutputBlobId)) // 如果Management Service没有该测试用例则上传
                            {
                                var outputCaseBlobId = await client.PutBlobAsync("case_output_" + jt.TestCaseID + ".txt", System.Text.Encoding.UTF8.GetBytes(jt.TestCase.Output));
                                jt.TestCase.OutputBlobId = outputCaseBlobId.ToString();
                            }
                            blobs.Add(new BlobInfo { Id = Guid.Parse(jt.TestCase.InputBlobId), Name = "input_" + caseIndex + ".txt" });
                            blobs.Add(new BlobInfo { Id = Guid.Parse(jt.TestCase.OutputBlobId), Name = "output_" + caseIndex + ".txt" });
                            ++caseIndex;
                        }
                        var stateMachineId = await client.PutStateMachineInstanceAsync("TyvjJudgeStateMachine", "http://tyvj.cn", blobs); // 创建StateMachine实例
                        status.StateMachineId = stateMachineId.ToString();
                        DbContext.SaveChanges();
                    }
                    else
                    {
                        foreach (var jt in status.JudgeTasks)
                        {
                            try
                            {
                                var group = SignalR.JudgeHub.GetNode();
                                if (group == null) return Content("No Online Judger");
                                SignalR.JudgeHub.context.Clients.Group(group).Judge(new CodeComb.Judge.Models.JudgeTask(jt));
                                SignalR.JudgeHub.ThreadBusy(group);
                            }
                            catch { }
                        }
                    }
                    SignalR.UserHub.context.Clients.Group("Status").onStatusCreated(new vStatus(status));//推送新状态
                }

                SignalR.UserHub.context.Clients.Group("Status").onStatusChanged(new vStatus(status));
                GC.Collect();
            }
            return Content("Done");
        }

        public ActionResult Show(int id)
        {
            ViewBag.CodeVisiable = false;
            var status = DbContext.Statuses.Find(id);
            if (User.Identity.IsAuthenticated)
            {
                if (CurrentUser.ID == status.UserID || CurrentUser.Role >= UserRole.Master || CurrentUser.ID == status.Problem.UserID || (status.ContestID != null && CurrentUser.ID == status.Contest.UserID))
                    ViewBag.CodeVisiable = true;
            }
            int MemoryUsage = 0, TimeUsage = 0;
            try
            {
                MemoryUsage = status.JudgeTasks.Max(x => x.MemoryUsage);
                TimeUsage = status.JudgeTasks.Sum(x => x.TimeUsage);
            }
            catch { }
            ViewBag.TimeUsage = TimeUsage;
            ViewBag.MemoryUsage = MemoryUsage;
            if (status.ContestID!=null&&status.Contest.Format == ContestFormat.OI && DateTime.Now < status.Contest.End)
            {
                if (User.Identity.IsAuthenticated == false)
                {
                    status.Result = JudgeResult.Hidden;
                    ViewBag.TimeUsage = 0;
                    ViewBag.MemoryUsage = 0;
                }
                else if (!(CurrentUser.Role >= UserRole.Master || CurrentUser.ID == status.Problem.UserID || (status.ContestID != null && CurrentUser.ID == status.Contest.UserID)))
                {
                    status.Result = JudgeResult.Hidden;
                    ViewBag.TimeUsage = 0;
                    ViewBag.MemoryUsage = 0;
                }
            }
            return View(status);
        }

        [HttpGet]
        public ActionResult GetStatusDetails(int id)
        {
            //TODO: 针对不同权限不同赛制提供有限的内容
            var status = DbContext.Statuses.Find(id);
            Contest contest = null;
            if (status.ContestID != null)
            { 
                contest = status.Contest;
            }
            var judgetasks = (from jt in DbContext.JudgeTasks
                              where jt.StatusID == id
                              select jt).ToList();
            var statusdetails = new List<vStatusDetail>();
            int index = 0;
            foreach (var jt in judgetasks)
                statusdetails.Add(new vStatusDetail(jt, index++));
            if (contest!=null && (DateTime.Now >= contest.End || (CurrentUser!=null && contest.UserID == CurrentUser.ID)))
                return Json(statusdetails, JsonRequestBehavior.AllowGet);
            if (contest != null && (contest.Format == ContestFormat.ACM || contest.Format == ContestFormat.OI))
            {
                return Json(new object(), JsonRequestBehavior.AllowGet);
            }
            else if (contest != null && contest.Format == ContestFormat.OI)
            {
                foreach (var sd in statusdetails)
                    sd.Hint = "比赛期间不提供详细信息显示";
                return Json(statusdetails, JsonRequestBehavior.AllowGet);
            }
            GC.Collect();
            return Json(statusdetails, JsonRequestBehavior.AllowGet);
        }
    }
}