using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tyvj.DataModels;
using Tyvj.ViewModels;

namespace Tyvj.Controllers
{
    public class HomeController : BaseController
    {
        public static List<vTopic> HomeTopicsCache = new List<vTopic>();

        public static void RefreshHomeTopicsCache()
        {
            System.Threading.Tasks.Task.Factory.StartNew(() => 
            {
                var DbContext = new DB();
                var _topics = (from t in DbContext.Topics
                               orderby t.LastReply descending
                               select t).ToList();
                if (_topics.Count > 10)
                {
                    _topics = _topics.Take(10).ToList();
                }
                var tmp = new List<vTopic>();
                foreach (var t in _topics)
                    tmp.Add(new vTopic(t));
                HomeTopicsCache = tmp;
            });
        }
        // GET: Home
        public ActionResult Index()
        {
            var contests = new List<vContest>();
            var _contests = (from c in ContestController.ContestListCache
                             where c.Begin <= DateTime.Now
                             && c.End > DateTime.Now
                             && c.ContestProblems.Count > 0
                             orderby c.Official descending
                             select c).ToList();

            if (_contests.Count < 5)
            {
                var time = DateTime.Now.AddDays(30);
                _contests = _contests.Union((from c in ContestController.ContestListCache
                                             where DateTime.Now < c.Begin
                                             && c.Begin < time
                                             && c.ContestProblems.Count > 0
                                             orderby c.Official descending, c.Begin ascending
                                             select c).Take(5 - _contests.Count).ToList()).ToList();
            }
            if (_contests.Count < 5)
            {
                _contests = _contests.Union((from c in ContestController.ContestListCache
                                             where DateTime.Now >= c.End
                                             && c.ContestProblems.Count > 0
                                             orderby c.End descending
                                             select c).Take(5 - _contests.Count).ToList()).ToList();
            }

            foreach (var c in _contests)
                contests.Add(new vContest(c));
            ViewBag.Topics = HomeTopicsCache;
            ViewBag.Contests = contests;
            return View();
        }
    }
}