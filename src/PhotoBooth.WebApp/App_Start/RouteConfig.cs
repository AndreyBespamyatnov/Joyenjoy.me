using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using PhotoBooth.WebApp.Routers;

namespace PhotoBooth.WebApp
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            //Маршрут идёт первым, чтобы не попал в IgnoreRoute.
            routes.Add("ImagesRoute", new Route("EventImages/{container}/{filename}", new ImageRouteHandler()));
            
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
