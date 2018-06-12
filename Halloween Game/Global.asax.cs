using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using StackExchange.Profiling;

namespace Halloween_Game {
	public class HGameApp : System.Web.HttpApplication {
		public static Random Rnd;

		public static Settings Settings;

		public static DateTime LastTaskUpdate {
			get { return HttpContext.Current.Application["lastTaskUpdate"] != null ? (DateTime)HttpContext.Current.Application["lastTaskUpdate"] : DateTime.MaxValue; }
			set {
				HttpContext.Current.Application.Lock();
				HttpContext.Current.Application["lastTaskUpdate"] = value;
				HttpContext.Current.Application.UnLock();
			}
		}

		static HGameApp() {
			Rnd = new Random();
			Settings = Settings.Load();
		}

		protected void Application_Start() {
			AreaRegistration.RegisterAllAreas();
			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			RouteConfig.RegisterRoutes(RouteTable.Routes);
			BundleConfig.RegisterBundles(BundleTable.Bundles);

			LastTaskUpdate = DateTime.Now.AddMinutes(-10);
			Halloween_Game.Session.StartNewSession();
		}

		protected void Application_BeginRequest() {
			if (Request.IsLocal) MiniProfiler.Start();
		}

		protected void Application_EndRequest() {
			MiniProfiler.Stop();
		}

		protected void Session_End(Object sender, EventArgs E) {
			// we don't have httpcontext here must less request cookies
			//Player.CurrentPlayer.idle = true;
			//Player.CurrentPlayer.Save();
		}
	}

	public class HGameAuth : AuthorizeAttribute {
		protected override bool AuthorizeCore(HttpContextBase httpContext) {
			return IsAuthorized;
		}

		protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext) {
			filterContext.Result = new RedirectResult("/Admin/Login");
		}

		/// <summary> True if current session is authorized for admin. </summary>
		public static bool IsAuthorized {
			get {
				HttpCookie authCookie = HttpContext.Current.Request.Cookies.Get("hgame_auth");
				return (authCookie != null && authCookie.Value == "True");
			}
		}

		public static bool Login(string pw) {
			if (pw != System.Configuration.ConfigurationManager.AppSettings["Admin Password"]) return false;

			HttpContext.Current.Response.SetCookie(new HttpCookie("hgame_auth", "True") { Expires = DateTime.Now.AddDays(30) });
			return true;
		}

		public static void Logout() {
			HttpContext.Current.Response.SetCookie(new HttpCookie("hgame_auth", null));
		}
	}
}