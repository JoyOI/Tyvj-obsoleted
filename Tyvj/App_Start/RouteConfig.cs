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
                name: "Problem",
                url: "p/{id}",
                defaults: new { controller = "Problem", action = "Show" },
                constraints: new { id = @"\d+" }
            );

            routes.MapRoute(
                name: "Status",
                url: "Status/{id}",
                defaults: new { controller = "Status", action = "Show" },
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
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
