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
            Name = group.Name;
            Description = group.Description;
            Gravatar = Helpers.Gravatar.GetAvatarURL(group.Gravatar, 200);
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public string Gravatar { get; set; }
    }
}