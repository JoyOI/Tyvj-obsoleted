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
            IEnumerable<Problem> _problems = (from p in DbContext.Problems
                                                     where (p.Title.Contains(Title))
                                                     select p);
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
            _problems = _problems.OrderBy(x => x.ID).Skip(100 * Page.Value).Take(100).ToList();
            List<vProblem> problems = new List<vProblem>();
            if (User.Identity.IsAuthenticated)
            {
                foreach (var problem in _problems)
                    problems.Add(new vProblem(problem, ViewBag.CurrentUser.Username));
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
            if (problem == null)
                return RedirectToAction("Message", "Shared", new { msg = "没有找到这道题目" });
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
            var _problems = (from p in DbContext.Problems
                             where p.Title.Contains(Title)
                             || Title.Contains(p.Title)
                             select p).ToList();
            var problems = new List<vExistedProblem>();
            foreach (var p in _problems)
                problems.Add(new vExistedProblem(p));
            return Json(problems, JsonRequestBehavior.AllowGet);
        }
    }
}