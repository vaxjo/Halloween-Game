using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Halloween_Game.Controllers {
	public class CentralController : Controller {
		public ActionResult Index() {
			return View();
		}

		public ActionResult NewsFeed() {
			return View();
		}

		public ActionResult Teams() {
			return View();
		}

		public JsonResult GetStatus() {
			return Json(new {
				state = Halloween_Game.Session.CurrentSession.State.ToString(),
				code = Halloween_Game.Session.CurrentSession.Code
			}, JsonRequestBehavior.AllowGet);
		}

		// periodic callback for general game management
		public ActionResult Update() {

			// time warp actions 
			if (Halloween_Game.Session.CurrentSession.State == Halloween_Game.Session.SessionState.TimeWarp) return TimeWarp();

			// reset
			if (Halloween_Game.Session.CurrentSession.State == Halloween_Game.Session.SessionState.Reset) {
				if (Halloween_Game.Session.CurrentSession.TimeSinceLastModified.TotalSeconds > 90) Halloween_Game.Session.StartNewSession();
				return null;
			}

			// look for any expired tasks
			PlayerTask.ExpireTasks();

			foreach (Team team in Team.GetAll()) team.StockWarehouse();

			// once every few seconds consider assign new tasks to players - high enough that we never have two of these executing simultaneously
			if (DateTime.Now.Subtract(HGameApp.LastTaskUpdate).TotalSeconds > 10) {
				foreach (var player in Player.GetAll()) PlayerTask.AssignTaskMaybe(player.id);
				HGameApp.LastTaskUpdate = DateTime.Now;
			}

			Newsfeed.AddNewsfeedItem();

			return null;
		}

		// routine that runs at the end of a game session
		private static ActionResult TimeWarp() {
			double secElapsed = Halloween_Game.Session.CurrentSession.TimeSinceLastModified.TotalSeconds;

			if (Halloween_Game.Session.TimeWarpLevel == 0) {
				PlayerItem finalItem = PlayerItem.Load((int)System.Web.HttpContext.Current.Application["TimeWarp PlayerItem Id"]);
				// all that initial stuff
				Newsfeed.Create("BREAKING - massive gravitation waves resulting from an experiment at <b>" + finalItem.GetPlayer.GetTeam.name + "</b> research laboratories is causing wide-spread damage and chaos through the local coordinated star systems.", Newsfeed.Context.danger);
				// global notification
				Notification.CreateGlobal("Anomalies in local spacetime have disrupted all normal activities.");

				// remove all player tasks & items
				foreach (Player player in Player.GetAll()) {
					foreach (PlayerTask playerTask in player.GetTasks) playerTask.Delete();
					foreach (PlayerItem playerItem in player.GetItems) playerItem.Delete();
					player.MustReload = true;
				}

				Halloween_Game.Session.TimeWarpLevel++;

			} else if (secElapsed > 10 && Halloween_Game.Session.TimeWarpLevel == 1) {
				Notification.CreateGlobal("Direct all queries to Central Display.");
				Halloween_Game.Session.TimeWarpLevel++;

			} else if (secElapsed > 30 && Halloween_Game.Session.TimeWarpLevel == 2) {
				Newsfeed.Create("Atomic chronometers reportedly becoming unreliable. Scientists puzzled. Clocks are running slowly, or stopping completely.", Newsfeed.Context.warning);
				Halloween_Game.Session.TimeWarpLevel++;

			} else if (secElapsed > 40 && Halloween_Game.Session.TimeWarpLevel == 3) {
				Newsfeed.Create("System-wide entropy decreasing. Natural processes have begun running backwards.", Newsfeed.Context.danger);
				Halloween_Game.Session.TimeWarpLevel++;

			} else if (secElapsed > 48 && Halloween_Game.Session.TimeWarpLevel == 4) {
				Newsfeed.Create("Some hope that a cure for aging has been discovered.", Newsfeed.Context.success);
				Notification.CreateGlobal("You feel yourself becoming younger. Knowledge is slipping from your mind. Mistakes and accomplishments are being unmade.");
				Halloween_Game.Session.TimeWarpLevel++;

			} else if (secElapsed > 64 && Halloween_Game.Session.TimeWarpLevel == 5) {
				Newsfeed.Create("Connection lost. Critical system failure imminent.", Newsfeed.Context.warning);
				Halloween_Game.Session.TimeWarpLevel++;

			} else if (secElapsed > 74 && Halloween_Game.Session.TimeWarpLevel == 6) {
				for (int i = 0; i < 7; i++) {
					Newsfeed.Create("GENERAL SYSTEM ERROR", Newsfeed.Context.danger);
				}
				Halloween_Game.Session.TimeWarpLevel++;

			} else if (secElapsed > 90 && Halloween_Game.Session.TimeWarpLevel == 7) {
				Halloween_Game.Session.CurrentSession.SetState(Halloween_Game.Session.SessionState.Reset);
				foreach (Player player in Player.GetAll()) {
					player.MustReload = true;
					player.Save();
				}
			}

			return null;
		}
	}
}