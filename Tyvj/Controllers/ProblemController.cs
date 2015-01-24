using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tyvj.DataModels;
using Tyvj.ViewModels;

namespace Tyvj.Controllers
{
    public class ProblemController : BaseController
    {
        public static List<Problem> ProblemListCache = new List<Problem>();
        // GET: Problem
        public ActionResult Index()
        {
            var tags = (from t in DbContext.AlgorithmTags
                        where t.FatherID == null
                        select t).ToList();
            ViewBag.Tags = tags;
            var pager = new List<vPager>();
            if (DbContext.Problems.Count() > 0)
            {
                var begin = DbContext.Problems.OrderBy(x => x.ID).First().ID;
                var end = DbContext.Problems.OrderByDescending(x => x.ID).First().ID;
                begin = begin / 100 * 100;
                if (end % 100 != 0)
                    end = end / 100 * 100 + 1;
                for (int i = begin; i <= end; i += 100)
                {
                    pager.Add(new vPager
                    {
                        Begin = i,
                        End = i + 99,
                        Display = "P" + i + ">"
                    });
                }
            }
            ViewBag.Pager = pager;
            return View();
        }

        public static void RefreshProblemListCache()
        {
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                var DbContext = new DataModels.DB();
                ProblemListCache = (from p in DbContext.Problems
                                    select p).ToList();
            });
        }

        [HttpGet]
        public ActionResult GetProblems(int? Page, string Title, string Tags, int? MoreThan, int? LessThan)
        { 
            if(Page==null)Page= 0;
            if(Title == null) Title = "";
            if (Tags == null) Tags = "";
            Tags = Tags.Trim(',').Trim(' ');
            var _tags = Tags.Split(',');
            var tags = new List<int>();
            if (Tags.Length > 0) 
            {
                foreach (var t in _tags)
                    tags.Add(Convert.ToInt32(t));
            }
            IEnumerable<Problem> _problems = (from p in ProblemListCache
                                                     where (p.Title.Contains(Title))
                                                     select p);
            if (!IsMaster())
                _problems = _problems.Where(x => x.Hide == false);
            if (!IsMaster() && User.Identity.IsAuthenticated)
                _problems = _problems.Where(x => x.Hide == false || x.UserID == CurrentUser.ID);
            if (tags.Count > 0)
            {
                var __problems = (from p in DbContext.SolutionTags
                                  where tags.Contains(p.AlgorithmTagID)
                                  select p.Solution.ProblemID).ToList();
                _problems = (from p in _problems
                             where __problems.Contains(p.ID)
                             select p);
            }
            if (MoreThan != null && LessThan != null)
                _problems = _problems.Where(x => x.ID >= MoreThan && x.ID <= LessThan);
            _problems = _problems.OrderBy(x => x.ID).Skip(100 * Page.Value).Take(100);
            _problems = _problems.ToList();
            List<vProblem> problems = new List<vProblem>();
            if (User.Identity.IsAuthenticated)
            {
                var ac = Helpers.AcList.GetList(CurrentUser.AcceptedList);
                var submit = Helpers.AcList.GetList(CurrentUser.SubmitList);
                foreach (var problem in _problems)
                    problems.Add(new vProblem(problem, ac, submit));
                    //problems.Add(new vProblem(problem));
            }
            else 
            {
                foreach (var problem in _problems)
                    problems.Add(new vProblem(problem));
            }
            return Json(problems, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Show(int? id, int? cpid)
        {
            Problem problem;
            Contest contest;
            int pid;
            ViewBag.ContestID = null;
            if (id != null)
            {
                problem = DbContext.Problems.Find(id.Value);
                pid = problem.ID;
                if (problem.Hide && (CurrentUser == null || !IsMaster() && CurrentUser.ID != problem.UserID))
                    return Message("没有找到题目");
            }
            else
            {
                var cp = DbContext.ContestProblems.Find(cpid.Value);
                problem = cp.Problem;
                contest = cp.Contest;
                pid = problem.ID;
                if (DateTime.Now >= contest.End)
                    return RedirectToAction("Show", "Problem", new { id = problem.ID });
                ViewBag.ContestID = cp.ContestID;
            }
            var uid = 0;
            if(User.Identity.IsAuthenticated)
            {
                uid = ViewBag.CurrentUser.ID;
            }
            var _statuses = (from s in DbContext.Statuses
                             where s.ProblemID == pid
                             && s.UserID == uid
                             orderby s.Time descending
                             select s).Take(20).ToList();
            if (cpid.HasValue)
            {
                foreach (var s in _statuses)
                    s.Result = JudgeResult.Hidden;
            }
            var statuses = new List<vProblemStatus>();
            foreach (var s in _statuses)
                statuses.Add(new vProblemStatus(s));
            ViewBag.Statuses = statuses;
            return View(problem);
        }

        [HttpGet]
        public ActionResult GetTitle(int id)
        {
            var problem = DbContext.Problems.Find(id);
            if (problem == null) return Content("");
            return Content(problem.Title);
        }

        [Authorize]
        public ActionResult Create()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetExistedProblems(string Title)
        {
            var _problems = (from p in ProblemListCache
                             where p.Title.Contains(Title)
                             || Title.Contains(p.Title)
                             select p).ToList();
            var problems = new List<vExistedProblem>();
            foreach (var p in _problems)
                problems.Add(new vExistedProblem(p));
            return Json(problems, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(string ProblemTitle)
        {
            var problem = new Problem 
            { 
                UserID = CurrentUser.ID,
                RangeValidator = "",
                SpecialJudge = "",
                StandardProgram = "",
                Official = false,
                MemoryLimit = 65536,
                TimeLimit = 1000,
                Title = ProblemTitle,
                Hide = true,
                Background = "",
                Description="请在此填写题目描述",
                Input = "请在此填写输入格式",
                Output = "请在此填写输出格式"
            };
            DbContext.Problems.Add(problem);
            DbContext.SaveChanges();
            RefreshProblemListCache();
            return RedirectToAction("Edit", "Problem", new { id = problem.ID });
        }

        [Authorize]
        public ActionResult Edit(int id)
        {
            var problem = DbContext.Problems.Find(id);
            if (!IsMaster() && problem.UserID != CurrentUser.ID)
                return Message("您无权执行本操作");
            return View(problem);
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult Edit(int id, string Title, int TimeLimit, int MemoryLimit, string Description, string Background, string Input, string Output, string Hint, bool? Official, bool Hide, int? Difficulty)
        {
            var problem = DbContext.Problems.Find(id);
            if (!IsMaster() && problem.UserID != CurrentUser.ID)
                return Content("False");
            problem.Title = Title;
            problem.Background = Background;
            problem.Description = Description;
            problem.Input = Input;
            problem.Output = Output;
            problem.Hint = Hint;
            problem.Hide = Hide;
            if (TimeLimit <= 2000 || IsMaster())
                problem.TimeLimit = TimeLimit;
            else
                problem.TimeLimit = 2000;
            if (MemoryLimit <= 131072 || IsMaster())
                problem.MemoryLimit = MemoryLimit;
            else
                problem.MemoryLimit = 131072;
            if (IsMaster())
            {
                problem.Official = Official.Value;
                problem.Difficulty = Difficulty.Value;
            }
            DbContext.SaveChanges();
            RefreshProblemListCache();
            return Content("True");
        }

        [Authorize]
        public ActionResult TestCase(int id)
        {
            var problem = DbContext.Problems.Find(id);
            if (!IsMaster() && problem.UserID != CurrentUser.ID)
                return Message("您无权执行本操作");
            return View(problem);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult DeleteTestCase(int id)
        {
            var testcase = DbContext.TestCases.Find(id);
            var problem_id = testcase.ProblemID;
            var problem = testcase.Problem;
            if (!IsMaster() && problem.UserID != CurrentUser.ID)
                return Message("您无权执行本操作");
            DbContext.TestCases.Remove(testcase);
            DbContext.SaveChanges();
            return RedirectToAction("TestCase", "Problem", new { id = problem_id });
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult ChangeTestCaseType(int tid, int type)
        {
            var testcase = DbContext.TestCases.Find(tid);
            var problem = testcase.Problem;
            if (!IsMaster() && problem.UserID != CurrentUser.ID)
                return Message("您无权执行本操作");
            testcase.TypeAsInt = type;
            DbContext.SaveChanges();
            return RedirectToAction("TestCase", "Problem", new { id = testcase.ProblemID });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult TestCaseEdit(int id, string input, string output)
        {
            var testcase = DbContext.TestCases.Find(id);
            var problem = testcase.Problem;
            if (!IsMaster() && problem.UserID != CurrentUser.ID)
                return Message("您无权执行本操作");
            testcase.Input = input;
            testcase.Output = output;
            DbContext.SaveChanges();
            return RedirectToAction("TestCase", "Problem", new { id = testcase.ProblemID });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult TestCaseTextUpload(int id, string input, string output)
        {
            var problem = DbContext.Problems.Find(id);
            if (!IsMaster() && problem.UserID != CurrentUser.ID)
                return Message("您无权执行本操作");
            DbContext.TestCases.Add(new TestCase
            {
                Input = input,
                Output = output,
                ProblemID = id,
                Type = TestCaseType.Overall,
                Hash = Helpers.Security.SHA1(input)
            });
            DbContext.SaveChanges();
            return RedirectToAction("TestCase", "Problem", new { id = problem.ID });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult TestCaseUpload(int id)
        {
            var problem = DbContext.Problems.Find(id);
            if (!IsMaster() && problem.UserID != CurrentUser.ID)
                return Message("您无权执行本操作");
            var file = Request.Files[0];
            var timestamp = Helpers.String.ToTimeStamp(DateTime.Now);
            var filename = timestamp + ".zip";
            var dir = Server.MapPath("~") + @"\Temp\";
            file.SaveAs(dir + filename);
            Helpers.Zip.UnZip(dir + filename, dir + timestamp);
            System.IO.File.Delete(dir + filename);
            var files = System.IO.Directory.GetFiles(dir + timestamp);
            foreach (var f in files)
            {
                if (System.IO.Path.GetExtension(f) == ".in")
                {
                    var testcase = new TestCase();
                    testcase.Input = System.IO.File.ReadAllText(f);
                    var outfile = f.Substring(0, f.Length - 3) + ".out";
                    var exist = false;
                    if (System.IO.File.Exists(outfile))
                    {
                        testcase.Output = System.IO.File.ReadAllText(outfile);
                        exist = true;
                    }
                    outfile = f.Substring(0, f.Length - 3) + ".ans";
                    if (System.IO.File.Exists(outfile))
                    {
                        testcase.Output = System.IO.File.ReadAllText(outfile);
                        exist = true;
                    }
                    if (!exist) continue;
                    testcase.ProblemID = id;
                    testcase.Type = TestCaseType.Overall;
                    testcase.Hash = Helpers.Security.SHA1(testcase.Input);
                    DbContext.TestCases.Add(testcase);
                }
            }
            DbContext.SaveChanges();
            System.IO.Directory.Delete(dir + timestamp, true);
            return RedirectToAction("TestCase", "Problem", new { id = problem.ID });
        }

        [HttpGet]
        [Authorize]
        public ActionResult GetTestCase(int id)
        {
            var testcase = DbContext.TestCases.Find(id);
            var problem = testcase.Problem;
            if (!IsMaster() && problem.UserID != CurrentUser.ID)
                return Json(null, JsonRequestBehavior.AllowGet);
            return Json(new vTestCase(testcase), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult Spj(int id)
        {
            var problem = DbContext.Problems.Find(id);
            if (!IsMaster() && CurrentUser.ID != problem.ID)
                return Message("您无权执行本操作");
            return View(problem);
        }

        [Authorize]
        public ActionResult Std(int id)
        {
            var problem = DbContext.Problems.Find(id);
            if (!IsMaster() && CurrentUser.ID != problem.ID)
                return Message("您无权执行本操作");
            return View(problem);
        }

        [Authorize]
        public ActionResult Range(int id)
        {
            var problem = DbContext.Problems.Find(id);
            if (!IsMaster() && CurrentUser.ID != problem.ID)
                return Message("您无权执行本操作");
            return View(problem);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Spj(int id, int language, string spj)
        {
            var problem = DbContext.Problems.Find(id);
            if (!IsMaster() && CurrentUser.ID != problem.ID)
                return Content("False");
            problem.SpecialJudge = spj;
            problem.SpecialJudgeLanguageAsInt = language;
            DbContext.SaveChanges();
            return Content("True");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Std(int id, int language, string std)
        {
            var problem = DbContext.Problems.Find(id);
            if (!IsMaster() && CurrentUser.ID != problem.ID)
                return Content("False");
            problem.StandardProgram = std;
            problem.StandardProgramLanguageAsInt = language;
            DbContext.SaveChanges();
            return Content("True");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Range(int id, int language, string range)
        {
            var problem = DbContext.Problems.Find(id);
            if (!IsMaster() && CurrentUser.ID != problem.ID)
                return Content("False");
            problem.RangeValidator = range;
            problem.RangeValidatorLanguageAsInt = language;
            DbContext.SaveChanges();
            return Content("True");
        }

        public ActionResult Solution(int id)
        {
            var problem = DbContext.Problems.Find(id);
            return View(problem);
        }
    }
}