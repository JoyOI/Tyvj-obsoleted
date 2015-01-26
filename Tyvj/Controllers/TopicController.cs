using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tyvj.ViewModels;
using Tyvj.DataModels;

namespace Tyvj.Controllers
{
    public class TopicController : BaseController
    {
        //
        // GET: /Topic/
        public ActionResult Index(int id)
        {
            var topic = DbContext.Topics.Find(id);
            return View(topic);
        }

        [HttpGet]
        public ActionResult GetTopics(int page, int? ForumID)
        {
            List<DataModels.Topic> _topics;
            if (ForumID == null)
            {
                _topics = (from t in DbContext.Topics
                           orderby t.LastReply descending 
                           select t).Skip(page * 10).Take(10).ToList();
            }
            else
            {
                _topics = (from t in DbContext.Topics
                           where t.ForumID == ForumID
                           orderby t.LastReply descending
                           select t).Skip(page * 10).Take(10).ToList();
            }
            List<ViewModels.vTopic> topics = new List<ViewModels.vTopic>();
            foreach (var topic in _topics)
                topics.Add(new ViewModels.vTopic(topic));
            return Json(topics, JsonRequestBehavior.AllowGet);
        }

       [Authorize]
        public ActionResult Create(int id)
        {
            if ((from f in DbContext.Forums where f.ID == id && f.FatherID != null select f).Count() == 0)
                return Message("没有找到这个论坛版块！" );
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ViewModels.vPost model)
        {
            if ((from f in DbContext.Forums where f.ID == model.ForumID && f.FatherID != null select f).Count() == 0)
                return Message("没有找到这个论坛版块！");
            if (string.IsNullOrEmpty(model.Content))
                return Message("内容不能为空！" );
            if (string.IsNullOrEmpty(model.Title))
                return Message("标题不能为空！");
            if (model.Reward > 0)
            {
                if (CurrentUser.Coins < model.Reward)
                    return Message("您的金币不足，无法发出悬赏。");
                else
                {
                    var user = DbContext.Users.Find(CurrentUser.ID);
                    user.Coins -= model.Reward;
                }
            }
            var topic = new DataModels.Topic
            {
                ForumID = model.ForumID,
                Title = model.Title,
                Content = model.Content,
                UserID = CurrentUser.ID,
                Time = DateTime.Now,
                Top = false,
                LastReply = DateTime.Now,
                Reward = model.Reward
            };
            DbContext.Topics.Add(topic);
            DbContext.SaveChanges();
            HomeController.RefreshHomeTopicsCache();
            return RedirectToAction("Index", "Topic", new { id = topic.ID });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var topic = DbContext.Topics.Find(id);
            if (topic.UserID == ViewBag.CurrentUser.ID || ((DataModels.User)ViewBag.CurrentUser).Role >= UserRole.Master)
            {
                var forum_id = topic.ForumID;
                DbContext.Topics.Remove(topic);
                DbContext.SaveChanges();
                return RedirectToAction("Index", "Forum", new { id = forum_id });
            }
            else
            {
                return Message("您无权删除这个主题！");
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, string content)
        {
            var topic = DbContext.Topics.Find(id);
            if (topic.UserID == ViewBag.CurrentUser.ID || ((DataModels.User)ViewBag.CurrentUser).Role >= UserRole.Master)
            {
                topic.Content = content;
                DbContext.SaveChanges();
                return Content("OK");
            }
            else
            {
                return Content("NO");
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UnSetTop(int id)
        {
            if (((DataModels.User)ViewBag.CurrentUser).Role >= UserRole.Master)
            {
                var topic = DbContext.Topics.Find(id);
                topic.Top = false;
                DbContext.SaveChanges();
                return Content("OK");
            }
            else
            {
                return Content("NO");
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SetTop(int id)
        {
            if (((DataModels.User)ViewBag.CurrentUser).Role >= UserRole.Master)
            {
                var topic = DbContext.Topics.Find(id);
                topic.Top = true;
                DbContext.SaveChanges();
                return Content("OK");
            }
            else
            {
                return Content("NO");
            }
        }

        [Authorize]
        [HttpGet]
        public ActionResult Reward(int id)
        {
            var reply = DbContext.Replies.Find(id);
            if (reply.Topic.UserID != CurrentUser.ID)
                return Message("您无权执行本操作！");
            if (reply.UserID == CurrentUser.ID)
                return Message("您不能采纳自己的回答！");
            reply.User.Coins += reply.Topic.Reward;
            reply.Awarded = true;
            DbContext.SaveChanges();
            return Message("采纳成功！");
        }
	}
}