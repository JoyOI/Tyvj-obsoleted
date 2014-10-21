using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tyvj.DataModels;
using Tyvj.ViewModels;

namespace Tyvj.Controllers
{
    public class NewsController : BaseController
    {
        // GET: News
        public ActionResult Index()
        {
            var news = (from n in DbContext.News
                        orderby n.Time descending
                        select n).ToList();
            return View(news);
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult Create(string Title, string Content)
        {
            if (!IsMaster())
                return Message("您无权执行本操作");
            DbContext.News.Add(new News 
            { 
                Time = DateTime.Now,
                Title = Title,
                Content = Content
            });
            DbContext.SaveChanges();
            return RedirectToAction("Index", "News", null);
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Delete(int id)
        {
            var news = DbContext.News.Find(id);
            DbContext.News.Remove(news);
            DbContext.SaveChanges();
            return RedirectToAction("Index", "News", null);
        }

        [Authorize]
        public ActionResult Edit(int id)
        {
            if (!IsMaster()) return Message("您无权执行本操作");
            var news = DbContext.News.Find(id);
            return View(news);
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult Edit(int id, string Title, string Content)
        {
            if (!IsMaster())
                return Message("您无权执行本操作");
            var news = DbContext.News.Find(id);
            news.Title = Title;
            news.Content = Content;
            DbContext.SaveChanges();
            return RedirectToAction("Index", "News", null);
        }
    }
}