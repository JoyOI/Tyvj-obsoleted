﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tyvj.DataModels;
using Tyvj.ViewModels;

namespace Tyvj.Controllers
{
    public class SolutionController : BaseController
    {
        // GET: Solution
        public ActionResult Index(int id)
        {
            var solution = DbContext.Solutions.Find(id);
            if (solution.Problem.Hide && (CurrentUser == null || (CurrentUser.ID != solution.UserID && CurrentUser.ID != solution.Problem.UserID && !IsMaster())))
                return Message("您无权执行本操作");
            return View(solution);
        }

        [Authorize]
        public ActionResult Create(int id)
        {
            var problem = DbContext.Problems.Find(id);
            if (problem.Hide && !IsMaster() && problem.UserID != CurrentUser.ID)
                return Message("您无权执行本操作");
            return View(problem);
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult Create(int id, string Title ,string Content, string Code, int language_id)
        {
            var problem = DbContext.Problems.Find(id);
            if (problem.Hide && !IsMaster() && problem.UserID != CurrentUser.ID)
                return Message("您无权执行本操作");
            var solution = new Solution 
            { 
                Code = Code,
                LanguageAsInt = language_id,
                ProblemID = id,
                Title = Title,
                UserID = CurrentUser.ID,
                Content = Content
            };
            DbContext.Solutions.Add(solution);
            DbContext.SaveChanges();
            return RedirectToAction("EditTags","Solution",new{id=solution.ID});
        }

        [HttpPost]
        [Authorize]
        public ActionResult SetTag(int id, int tid)
        {
            var solution = DbContext.Solutions.Find(id);
            if (solution.Problem.Hide && (CurrentUser == null || (CurrentUser.ID != solution.UserID && CurrentUser.ID != solution.Problem.UserID && !IsMaster())))
                return Content("Failed");
            var tags = solution.SolutionTags.Where(x => x.AlgorithmTagID == tid).ToList();
            if (tags.Count == 0)
            {
                var tag = new SolutionTag
                {
                    AlgorithmTagID = tid,
                    SolutionID = id
                };
                DbContext.SolutionTags.Add(tag);
                DbContext.SaveChanges();
                return Content("Added");
            }
            else
            {
                foreach (var tag in tags)
                    DbContext.SolutionTags.Remove(tag);
                DbContext.SaveChanges();
                return Content("Deleted");
            }
        }

        [Authorize]
        public ActionResult EditTags(int id)
        {
            var solution = DbContext.Solutions.Find(id);
            if (CurrentUser == null || (CurrentUser.ID != solution.UserID && CurrentUser.ID != solution.Problem.UserID && !IsMaster()))
                return Message("您无权执行本操作");
            ViewBag.Tags = (from at in DbContext.AlgorithmTags
                            where at.FatherID == null
                            orderby at.ID ascending
                            select at).ToList();
            return View(solution);
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult Edit(int id, string Title, string Content, string Code, int language_id)
        {
            var solution = DbContext.Solutions.Find(id);
            if (CurrentUser == null || (CurrentUser.ID != solution.UserID && CurrentUser.ID != solution.Problem.UserID && !IsMaster()))
                return Message("您无权执行本操作");
            solution.Title = Title;
            solution.Content = Content;
            solution.Code = Code;
            solution.LanguageAsInt = language_id;
            DbContext.SaveChanges();
            return RedirectToAction("EditTags", "Solution", new { id = solution.ID });
        }

        [HttpGet]
        public ActionResult GetSolutions(int? Page, int ProblemID)
        {
            if (Page == null) Page = 0;
            var _solutions = (from s in DbContext.Solutions
                              where s.ProblemID == ProblemID
                              orderby s.ID descending
                              select s).Skip(10 * Page.Value).Take(10).ToList();
            var solutions = new List<vSolution>();
            foreach (var s in _solutions)
                solutions.Add(new vSolution(s));
            return Json(solutions, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult Edit(int id)
        {
            var solution = DbContext.Solutions.Find(id);
            if (CurrentUser == null || (CurrentUser.ID != solution.UserID && CurrentUser.ID != solution.Problem.UserID && !IsMaster()))
                return Message("您无权执行本操作");
            return View(solution);
        }
    }
}