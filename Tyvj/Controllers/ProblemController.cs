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

        public ActionResult Show(int id)
        {
            var problem = DbContext.Problems.Find(id);
            if (problem == null)
                return RedirectToAction("Message", "Shared", new { msg = "没有找到这道题目" });
            var uid = 0;
            if(User.Identity.IsAuthenticated)
            {
                uid = ViewBag.CurrentUser.ID;
            }
            var _statuses = (from s in DbContext.Statuses
                             where s.ProblemID == id
                             && s.UserID == uid
                             orderby s.Time descending
                             select s).Take(20).ToList();
            var statuses = new List<vProblemStatus>();
            foreach (var s in _statuses)
                statuses.Add(new vProblemStatus(s));
            ViewBag.Statuses = statuses;
            return View(problem);
        }
    }
}