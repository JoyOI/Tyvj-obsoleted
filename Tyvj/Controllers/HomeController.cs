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
        // GET: Home
        public ActionResult Index()
        {
            var contests = new List<vContest>();
            var topics = new List<vTopic>();
            var _contests = (from c in DbContext.Contests
                             where c.Begin <= DateTime.Now
                             && c.End > DateTime.Now
                             && c.ContestProblems.Count > 0
                             select c).ToList();

            var _topics = (from t in DbContext.Topics
                           orderby t.LastReply descending
                           select t).ToList();


            if (_contests.Count < 5)
            {
                var time = DateTime.Now.AddDays(30);
                _contests = _contests.Union((from c in DbContext.Contests
                                             where DateTime.Now < c.Begin
                                             && c.Begin < time
                                             && c.ContestProblems.Count > 0
                                             orderby c.Begin ascending
                                             select c).Take(5 - _contests.Count).ToList()).ToList();
            }
            if (_contests.Count < 5)
            {
                _contests = _contests.Union((from c in DbContext.Contests
                                             where DateTime.Now >= c.End
                                             && c.ContestProblems.Count > 0
                                             orderby c.Begin ascending
                                             select c).Take(5 - _contests.Count).ToList()).ToList();
            }

            if (_topics.Count >10)
            {
                _topics = _topics.Take(10).ToList();
            }


            foreach (var c in _contests)
                contests.Add(new vContest(c));

            foreach (var t in _topics)
                topics.Add(new vTopic(t));

            ViewBag.Topics = topics;
            ViewBag.Contests = contests;
            return View();
        }
    }
}