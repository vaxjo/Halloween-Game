using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Halloween_Game {
	public class RouteConfig {
		public static void RegisterRoutes(RouteCollection routes) {
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.MapRoute("Join", "Join/{session}", new { controller = "Team", action = "Join", session = UrlParameter.Optional });

			routes.MapRoute("Default", "{controller}/{action}", new { controller = "Player", action = "Index" });
		}
	}
}
