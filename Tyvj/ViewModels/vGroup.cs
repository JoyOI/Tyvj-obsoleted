using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tyvj.DataModels;

namespace Tyvj.ViewModels
{
    public class vGroup
    {
        public vGroup() { }
        public vGroup(Group group)
        {
            ID = group.ID;
            Title = HttpUtility.HtmlEncode(group.Title);
            Description = HttpUtility.HtmlEncode(group.Description);
            Gravatar = Helpers.Gravatar.GetAvatarURL(group.Gravatar, 200);
            if (Description.Length > 51)
                Description = Description.Substring(0, 50) + "...";
            Time = group.Time.ToString("yyyy-MM-dd");
            MemberCount = group.Members.Count.ToString();
            Username = Helpers.ColorName.GetNicknameHtml(group.User.Username, group.User.Role);
            UserID = group.UserID;
        }

        public int ID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Gravatar { get; set; }
        public string Time { get; set; }
        public string MemberCount { get; set; }
        public string Username { get; set; }
        public int UserID { get; set; }
    }
}