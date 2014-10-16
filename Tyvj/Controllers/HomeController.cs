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
            var _contests = (from c in DbContext.Contests
                             where c.Begin <= DateTime.Now
                             && c.End > DateTime.Now
                             select c).ToList();
            if (_contests.Count < 5)
            {
                _contests = _contests.Union((from c in DbContext.Contests
                                             where DateTime.Now < c.Begin
                                             orderby c.Begin ascending
                                             select c).Take(5 - _contests.Count).ToList()).ToList();
            }
            if (_contests.Count < 5)
            {
                _contests = _contests.Union((from c in DbContext.Contests
                                             where DateTime.Now >= c.End
                                             orderby c.Begin ascending
                                             select c).Take(5 - _contests.Count).ToList()).ToList();
            }
            foreach (var c in _contests)
                contests.Add(new vContest(c));
            ViewBag.Contests = contests;
            return View();
        }
    }
}