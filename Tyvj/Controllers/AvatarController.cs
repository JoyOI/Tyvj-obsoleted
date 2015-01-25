using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Tyvj.Controllers
{
    public class AvatarController : BaseController
    {
        // GET: Avatar
        [OutputCache(Duration = 3600)]
        public ActionResult Index(int id)
        {
            var user = DbContext.Users.Find(id);
            if (user == null)
                return File(new byte[0], "image/png");
            if (user.Avatar.Count() == 0)
            {
                Helpers.Gravatar.RefreshGravatar(user.ID);
                user = DbContext.Users.Find(id);
            }
            return File(user.Avatar, "image/png");
        }
    }
}