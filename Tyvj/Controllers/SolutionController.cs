using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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
            return View();
        }
    }
}