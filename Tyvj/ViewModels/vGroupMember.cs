using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tyvj.DataModels;

namespace Tyvj.ViewModels
{
    public class vGroupMember
    {
        public vGroupMember() { }
        public vGroupMember(GroupMember gm)
        {
            UserID = gm.UserID;
            User = gm.User;
        }

        public int UserID { get; set; }
        public User User { get; set; }
    }
}