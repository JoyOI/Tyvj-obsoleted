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
<<<<<<< HEAD
            ID = group.ID;
            Title = group.Title;
=======
            Name = group.Title;
>>>>>>> 6d7730fe46eb36160dd9324abc1e2be10d9d056f
            Description = group.Description;
            Gravatar = Helpers.Gravatar.GetAvatarURL(group.Gravatar, 200);
        }

        public int ID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Gravatar { get; set; }
    }
}