using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Tyvj.ViewModels
{
    public class vTopic
    {
        public vTopic() { }
        public vTopic(DataModels.Topic topic)
        {
            ID = topic.ID;
            ForumID = topic.ForumID;
            ForumTitle = topic.Forum.Title;
            Gravatar = "/Avatar/" + topic.UserID;
            Nickname = Helpers.ColorName.GetNicknameHtml(topic.User.Username, topic.User.Role);
            RepliesCount = topic.Replies.Count;
            Time = topic.LastReply;
            Title = HttpUtility.HtmlEncode(topic.Title);
            Top = topic.Top;
            UserID = topic.UserID;
            HasReply = topic.Replies.Count == 0 ? false : true;
            LastReplyNickname = topic.Replies.Count == 0 ? null : topic.Replies.OrderBy(x => x.Time).Last().User.Username;
            LastReplyUserID = topic.Replies.Count == 0 ? null : (int?)(topic.Replies.OrderBy(x => x.Time).Last().UserID);
            Reward = topic.Reward;
        }
        public int ID { get; set; }
        public string Nickname { get; set; }
        public int RepliesCount { get; set; }
        public int ForumID { get; set; }
        public string ForumTitle { get; set; }
        public bool Top { get; set; }
        public string Title { get; set; }
        public int UserID { get; set;}
        public string Gravatar { get; set; }
        public bool HasReply { get; set; }
        public string LastReplyTime { get { return  Helpers.Time.ToTimeTip(Time); } }
        public string LastReplyNickname { get; set; }
        public int? LastReplyUserID { get; set; }
        public DateTime Time { get; set; }
        public int Reward { get; set; }
    }
}