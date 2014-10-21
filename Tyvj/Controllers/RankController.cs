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
            var users = (from s in DbContext.Statuses
                         group s by s.UserID
                         into t
                         select new
                         {
                             UserID = t.Key,
                             AC = t.Sum(x => x.ResultAsInt == 0 ? 1 : 0),
                             Count = t.Count()
                         }).OrderByDescending(x => x.AC).OrderBy(x => x.Count).Skip(10 * page).Take(10).ToList();
            List<vRank> ranks = new List<vRank>();
            for (int i = 0; i < users.Count(); i++)
                ranks.Add(new vRank(DbContext.Users.Find(users[i].UserID), page * 10 + i + 1));
            return Json(ranks, JsonRequestBehavior.AllowGet);
        }
    }
}