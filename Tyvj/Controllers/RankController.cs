using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tyvj.DataModels;
using Tyvj.ViewModels;

namespace Tyvj.Controllers
{
    public class RankController : BaseController
    {
        // GET: Rank
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetRanksByRating(int page)
        {
            var users = (from u in DbContext.Users
                         orderby u.Ratings.Sum(x => x.Credit) descending
                         select u).Skip(10 * page).Take(10).ToList();
            List<vRank> ratings = new List<vRank>();
            for (int i = 0; i < users.Count(); i++)
                ratings.Add(new vRank(users[i], page * 10 + i + 1));
            return Json(ratings, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetRanksByAC(int page)
        {
            var users = (from u in DbContext.Users
                         where u.SubmitCount > 0
                         orderby u.AcceptedCount descending, u.SubmitCount ascending
                         select u).Skip(page * 10).Take(10).ToList();
            List<vRank> ranks = new List<vRank>();
            for (int i = 0; i < users.Count(); i++)
                ranks.Add(new vRank(users[i], page * 10 + i + 1));
            return Json(ranks, JsonRequestBehavior.AllowGet);
        }
    }
}