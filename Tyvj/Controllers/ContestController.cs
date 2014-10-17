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
        // GET: Contest
        public ActionResult Index()
        {
            var _contests = (from c in DbContext.Contests
                                              where DateTime.Now >= c.Begin && DateTime.Now < c.End
                                              orderby c.End ascending
                                              select c).ToList();
            var contests = new List<vContest>();
            foreach (var c in _contests)
                contests.Add(new vContest(c));
            return View(contests);
        }

        [HttpGet]
        public ActionResult GetContests(int? Page, string Title, int? Format)
        { 
            if(Title == null)Title = "";
            if(Page == null)Page = 0;
            IEnumerable<Contest> _contests = (from c in DbContext.Contests
                                where c.Title.Contains(Title)
                                && !(DateTime.Now >= c.Begin && DateTime.Now <c.End)
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
            return View(contest);
        }

        [HttpGet]
        public ActionResult GetStandings(int id)
        {
            var user = ViewBag.CurrentUser == null ? new User() : (User)ViewBag.CurrentUser;
            var contest = DbContext.Contests.Find(id);
            if (!Helpers.Contest.UserInContest(user.ID, id))
                return RedirectToAction("Register", "Contest", new { id = id });
            if (contest.Format == ContestFormat.OI && DateTime.Now < contest.End && !ViewBag.IsMaster)
                return Json(null, JsonRequestBehavior.AllowGet);
            var standings = Helpers.Standings.Build(id);
            return Json(standings, JsonRequestBehavior.AllowGet);
        }
    }
}