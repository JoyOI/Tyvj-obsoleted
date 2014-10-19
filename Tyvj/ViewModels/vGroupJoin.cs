using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tyvj.DataModels;

namespace Tyvj.ViewModels
{
    public class vGroupJoin
    {
        public vGroupJoin() { }
        public vGroupJoin(GroupJoin groupJoin)
        {
            ID = groupJoin.ID;
            UserID = groupJoin.UserID;
            User = groupJoin.User;
            Content = groupJoin.Content;
            Time = groupJoin.Time;
        }

        public int ID { get; set; }
        public int UserID { get; set; }
        public User User { get; set; }
        public string Content { get;set; }
        public DateTime Time { get; set; }
    }
}