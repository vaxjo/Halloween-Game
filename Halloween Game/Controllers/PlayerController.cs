using System;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Halloween_Game.Controllers {
	public class PlayerController : Controller {
		public ActionResult Index() {
			if (Halloween_Game.Session.CurrentSession.State == Halloween_Game.Session.SessionState.Reset)
				return View("Index_Reset");

			if (Player.CurrentPlayer == null) return View("Index_None");

			Player.CurrentPlayer.MustReload = false;
			return View();
		}

		public class PlayerStatus_Model {
			public struct Task {
				public int id; public string name; public double duration, startedAgo;
			}
			public struct Item {
				public int id, size; public string itemId, name;
			}

			public Player_JSON player;
			public List<Task> tasks;
			public List<Item> items;
			public List<Notification_JSON> notifications;
			public bool mustReload;
			public bool idle;
		}

		/// <summary> Get the summary status of the current player. </summary>
		public JsonResult GetCurrentPlayerStatus() {
			if (Player.CurrentPlayer == null) return Json(new PlayerStatus_Model() { mustReload = true }, JsonRequestBehavior.AllowGet);

			PlayerStatus_Model model = new PlayerStatus_Model() {
				tasks = Player.CurrentPlayer.GetTasks.Select(o => new PlayerStatus_Model.Task() {
					id = o.id, name = o.GetTask.name,
					duration = o.GetTask.Duration.TotalSeconds,
					startedAgo = DateTime.Now.Subtract(o.assigned).TotalMilliseconds
				}).ToList(),
				items = Player.CurrentPlayer.GetItems.Take(4).Select(o => new PlayerStatus_Model.Item() { id = o.id, size = o.GetItem.size, itemId = o.GetItem.id, name = o.GetItem.name }).ToList(),
				notifications = Notification.GetAllForPlayer(Player.CurrentPlayerId.Value).Where(o => !Player.CurrentPlayer.ReadNotifications.Contains(o.id)).OrderByDescending(o => o.created).Select(o => o.GetJSON).ToList(),
				mustReload = Player.CurrentPlayer.MustReload,
				idle = Player.CurrentPlayer.idle,

				player = Player.CurrentPlayer.GetJSON
			};

			return Json(model, JsonRequestBehavior.AllowGet); // it's apparently slightly bad to send a JSON array to a GET request
		}

		/// <summary> Get the status of a specific player. </summary>
		public JsonResult GetPlayerStatus(string agentCode) {
			Player player = Player.Load(agentCode);
			if (player == null) return Json(new Player_JSON() { id = Guid.Empty }, JsonRequestBehavior.AllowGet);

			return Json(player.GetJSON, JsonRequestBehavior.AllowGet);
		}

		public EmptyResult NotificationRead(int id) {
			if (Player.CurrentPlayer.ReadNotifications.Contains(id)) return null;

			Player.CurrentPlayer.ReadNotifications.Add(id);
			Player.CurrentPlayer.Save();

			return null;
		}

		public ActionResult Modal_Profile() {
			if (Player.CurrentPlayer == null) return Content("");
			return View();
		}

		public ActionResult Modal_Task(int playerTaskId) {
			if (Player.CurrentPlayer == null) return Content("");
			return View(PlayerTask.Load(playerTaskId));
		}

		public ActionResult Modal_Item(int playerItemId) {
			if (Player.CurrentPlayer == null) return Content("");
			return View(PlayerItem.Load(playerItemId));
		}

		public ActionResult Modal_EmptyItem(bool? request) {
			if (request.HasValue && request.Value) {
				PlayerTask.CreateTask("acquisition", Player.CurrentPlayer.id);
				return null;
			}

			if (Player.CurrentPlayer == null) return Content("");
			return View();
		}	
	}
}