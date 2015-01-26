using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Tyvj
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "UserStatuses",
                url: "User/{uid}/Statuses",
                defaults: new { controller = "Status", action = "Index" },
                constraints: new { uid = @"\d+" }
            );

            routes.MapRoute(
                name: "ContestStatuses",
                url: "Contest/{cid}/Statuses",
                defaults: new { controller = "Status", action = "Index" },
                constraints: new { cid = @"\d+" }
            );

            routes.MapRoute(
               name: "Verifying",
               url: "{controller}/{action}/{id}/{token}",
               defaults: new { controller = "Verify", action = "Register" }
           );

            routes.MapRoute(
                name: "Problem",
                url: "p/{id}",
                defaults: new { controller = "Problem", action = "Show" },
                constraints: new { id = @"\d+" }
            );

            routes.MapRoute(
               name: "ContestProblem",
               url: "p/contest/{cpid}",
               defaults: new { controller = "Problem", action = "Show" },
               constraints: new { cpid=@"\d+" }
           );

            routes.MapRoute(
               name: "ProblemStatuses",
               url: "p/{pid}/statuses",
               defaults: new { controller = "Status", action = "Index" },
               constraints: new { pid = @"\d+" }
           );

            routes.MapRoute(
               name: "ProblemComments",
               url: "p/{id}/comment",
               defaults: new { controller = "Problem", action = "Comment" },
               constraints: new { id = @"\d+" }
           );

            routes.MapRoute(
                name: "Status",
                url: "Status/{id}",
                defaults: new { controller = "Status", action = "Show" },
                constraints: new { id = @"\d+" }
            );

            routes.MapRoute(
                name: "Avatar",
                url: "Avatar/{id}",
                defaults: new { controller = "Avatar", action = "Index" },
                constraints: new { id = @"\d+" }
            );

            routes.MapRoute(
               name: "Group",
               url: "Group/{id}",
               defaults: new { controller = "Group", action = "Show" },
               constraints: new { id = @"\d+" }
           );

            routes.MapRoute(
                name: "User",
                url: "User/{id}",
                defaults: new { controller = "User", action = "Index" },
                constraints: new { id = @"\d+" }
            );

            routes.MapRoute(
                name: "Contest",
                url: "Contest/{id}",
                defaults: new { controller = "Contest", action = "Show" },
                constraints: new { id = @"\d+" }
            );

            routes.MapRoute(
                name: "Login",
                url: "Login",
                defaults: new { controller = "User", action = "Login" }
            );

            routes.MapRoute(
                name: "Register",
                url: "Register",
                defaults: new { controller = "User", action = "Register" }
            );

            routes.MapRoute(
                name: "C-I-A",
                url: "{controller}/{id}/{action}",
                defaults: new { controller = "Home", action = "Index" },
                constraints: new { id = @"\d+" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
