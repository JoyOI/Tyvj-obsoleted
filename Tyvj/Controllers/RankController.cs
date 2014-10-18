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
        public ActionResult GetRanks(int page)
        {
            var users = (from u in DbContext.Users
                         orderby u.Ratings.Sum(x => x.Credit) descending
<<<<<<< HEAD
                         select u).Skip(12 * page).Take(12).ToList();
            List<Rating> ratings = new List<Rating>();
            for (int i = 0; i < users.Count(); i++)
                ratings.Add(new Rating(users[i], page * 12 + i + 1));
=======
                         select u).Skip(10 * page).Take(10).ToList();
            List<vRank> ratings = new List<vRank>();
             for (int i = 0; i < users.Count(); i++)
                 ratings.Add(new vRank(users[i], page * 10 + i + 1));
>>>>>>> ae09403613a823f0b149935f257a8dd4c97ffa27
            return Json(ratings, JsonRequestBehavior.AllowGet);
        }
    }
}