using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tyvj.DataModels;
using Tyvj.ViewModels;

namespace Tyvj.Controllers
{
    public class GroupController : BaseController
    {
        // GET: Group
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Show(int id)
        {
            var group = DbContext.Groups.Find(id);
            var _contests = (from gc in DbContext.GroupContest
                             where gc.GroupID == id
                             orderby gc.Contest.End descending
                             select gc.Contest).Take(5).ToList();
            var contests = new List<vContest>();
            foreach (var c in _contests)
                contests.Add(new vContest(c));
            ViewBag.Contests = contests;
            return View(group);
        }
    }
}