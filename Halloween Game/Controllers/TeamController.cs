using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Halloween_Game.Controllers {
	public class TeamController : Controller {

		public ActionResult Index(string team) {
			if (!Request.IsLocal && string.IsNullOrWhiteSpace(team)) team = GetTeamFromHostname();

			if (string.IsNullOrWhiteSpace(team)) return View("Summary");

			return View("Index_" + Halloween_Game.Session.CurrentSession.State.ToString(), Team.Load(team));
		}

		public ActionResult Summary() {
			return View();
		}

		// the actual route for this is /Join/{session}
		public ActionResult Join(string session, string team) {
			if (Player.CurrentPlayer != null) return Redirect("/");

			if (session.ToLower() != Halloween_Game.Session.CurrentSession.Code.ToLower()) return HttpNotFound(); // session code is wrong

			if (!Request.IsLocal) team = GetTeamFromHostname();

			ViewBag.SessionCode = session;
			return View(Team.Load(team));
		}

		public ActionResult Confirm(string session, string team) {
			if (Player.CurrentPlayer != null) return Redirect("/");

			if (session.ToLower() != Halloween_Game.Session.CurrentSession.Code.ToLower()) return Redirect("/");  // session code is wrong

			Session["hgamePlayerId"] = Player.Create(Team.Load(team)).id;

			return Redirect("/");
		}

		private string GetTeamFromHostname() {
			return Request.Url.Host.Split('.')[0]; // "ccc.jarrin.net"
		}
	}
}