using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Halloween_Game.Controllers {
	[HGameAuth]
	public class AdminController : Controller {

		[OverrideAuthorization]
		public ActionResult Login(string pw) {
			if (!string.IsNullOrWhiteSpace(pw) && HGameAuth.Login(pw)) return Redirect("/Admin");
			return View();
		}

		public ActionResult Logout() {
			HGameAuth.Logout();
			return Redirect("/Admin/Login");
		}

		public ActionResult Index(string page) {
			if (string.IsNullOrWhiteSpace(page)) return View();
			return View("Index_" + page);
		}

		#region Players

		public ActionResult Modal_Player(Guid id) {
			return View(Player.Load(id)); // we set the id on the new player here so we can use its hash to display the player name
		}

		public EmptyResult ChangeTeam(Guid playerId, string teamId) {
			Player player = Player.Load(playerId);
			player.Team = Team.Load(teamId);
			player.Save();

			player.MustReload = true;

			return null;
		}

		public JsonResult ChangeRank(Guid playerId, int rank) {
			Player player = Player.Load(playerId);
			player.Rank = rank;
			player.Save();

			player.MustReload = true;

			return Json(player.Role);
		}

		public EmptyResult NewFakePlayer() {
			Team t = Team.GetAll()[HGameApp.Rnd.Next(Team.GetAll().Count)];
			Player.Create(t);

			return null;
		}

		public ActionResult Modal_Player_GetTasks(Guid id) {
			return View(Player.Load(id));
		}

		public EmptyResult AssignTask(string id, Guid playerId) {
			PlayerTask.CreateTask(id, playerId);
			return null;
		}

		public EmptyResult KillTask(int id) {
			PlayerTask pt = PlayerTask.Load(id);
			pt.Delete();

			return null;
		}

		public ActionResult Modal_Player_GetItems(Guid id) {
			return View(Player.Load(id));
		}

		public EmptyResult AddItem(string itemId, Guid playerId) {
			Player player = Player.Load(playerId);
			player.AddItem(itemId);
			return null;
		}

		public EmptyResult RemoveItem(int id) {
			PlayerItem playerItem = PlayerItem.Load(id);
			playerItem.Delete();

			return null;
		}

		public EmptyResult DeletePlayer(Guid id) {
			Player Player = Player.Load(id);
			Player.Delete();
			return null;
		}

		public EmptyResult SwitchTeam(string teamId, Guid playerId) {
			Player player = Player.Load(playerId);
			player.Team = Team.Load(teamId);
			player.Save();
			return null;
		}

		#endregion

		#region Newsfeed

		public ActionResult Modal_Newsfeed_View(int id) {
			return View(Newsfeed.Load(id));
		}

		public EmptyResult DeleteNewsfeed(int id) {
			Newsfeed note = Newsfeed.Load(id);
			note.Delete();
			return null;
		}

		public ActionResult Modal_Newsfeed_Add() {
			return View();
		}

		[ValidateInput(false)]
		public EmptyResult AddNewsfeed(string recipient, string body, Newsfeed.Context context) {
			Newsfeed.Create(body, context);
			return null;
		}

		#endregion

		#region Notification

		public ActionResult Modal_Notification_View(int id) {
			return View(Notification.Load(id));
		}

		public EmptyResult DeleteNotification(int id) {
			Notification note = Notification.Load(id);
			note.Delete();
			return null;
		}

		public ActionResult Modal_Notification_Add() {
			return View();
		}

		[ValidateInput(false)]
		public EmptyResult AddNotification(string recipient, string message) {
			Guid recipientPlayerId;

			if (string.IsNullOrWhiteSpace(recipient)) Notification.CreateGlobal(message);
			else if (Guid.TryParse(recipient, out recipientPlayerId)) Notification.CreatePlayer(message, recipientPlayerId);
			else Notification.CreateTeam(message, recipient);

			return null;
		}

		#endregion

		#region Tasks

		public ActionResult Modal_Task(string id) {
			return View(string.IsNullOrWhiteSpace(id) ? new Task() { active = true } : Task.Load(id));
		}

		public EmptyResult UpdateTask(string id, string name, bool active, bool manual, int duration, int minPlayerRank, int minTechLevel) {
			if (string.IsNullOrWhiteSpace(id)) return null;

			Task task = Task.Load(id);
			if (task == null) task = new Task();

			task.id = id;
			task.name = name;
			task.active = active;
			task.manual = manual;
			task.duration = duration;
			task.minPlayerRank = minPlayerRank;
			task.minTechLevel = minTechLevel;
			task.Save();

			return null;
		}

		public EmptyResult DeleteTask(string id) {
			Task task = Task.Load(id);
			task.Delete();
			return null;
		}

		#endregion

		#region Items

		public ActionResult Modal_Item(string id) {
			return View(string.IsNullOrWhiteSpace(id) ? new Item() : Item.Load(id));
		}

		public EmptyResult UpdateItem(string id, string name, string description, short size, bool unique) {
			Item item = Item.Load(id);
			if (item == null) item = new Item();

			item.id = id;
			item.name = name;
			item.description = description;
			item.size = size;
			item.unique = unique;
			item.Save();

			return null;
		}

		public EmptyResult DeleteItem(string id) {
			Item item = Item.Load(id);
			item.Delete();
			return null;
		}

		#endregion

		#region Teams

		public ActionResult Modal_Team(string id) {
			return View(string.IsNullOrWhiteSpace(id) ? new Team() : Team.Load(id));
		}

		public ActionResult Modal_Team_GetWarehouse(string id) {
			return View(Team.Load(id));
		}

		public EmptyResult AddWarehouseItem(string itemId, string teamId) {
			Team team = Team.Load(teamId);
			team.WarehouseAddItem(itemId);
			return null;
		}

		public EmptyResult RemoveWarehouseItem(string itemId, string teamId) {
			Team team = Team.Load(teamId);
			team.WarehouseRetrieveItem(itemId);
			return null;
		}

		public EmptyResult UpdateTeam(string id, string name, string description, int score) {
			Team team = Team.Load(id);
			if (team == null) team = new Team();

			team.id = id;
			team.name = name;
			team.score = score;
			team.description = description;
			team.Save();

			return null;
		}

		public EmptyResult DeleteTeam(string id) {
			Team team = Team.Load(id);
			team.Delete();
			return null;
		}

		#endregion

		#region Session

		public EmptyResult SetState(Halloween_Game.Session.SessionState state) {
			Halloween_Game.Session.CurrentSession.SetState(state);
			return null;
		}

		public EmptyResult NewSession() {
			Halloween_Game.Session.StartNewSession();
			return null;
		}

		public EmptyResult DeleteAllSessions() {
			hgameDataContext dc = hgameDataContext.GetDataContext();
			dc.Sessions.DeleteAllOnSubmit(dc.Sessions);
			dc.SubmitChanges();

			Halloween_Game.Session.StartNewSession();

			return EmptyCache();
		}

		public EmptyResult EmptyCache() {
			Myriads.Cache.Empty();
			return null;
		}

		#endregion

		#region  Settings

		public EmptyResult UpdateSettings(bool devMode, double taskRate, int techRate, double durationMod) {
			HGameApp.Settings.DevMode = devMode;
			HGameApp.Settings.TaskRate = taskRate;
			HGameApp.Settings.TechRate = techRate;
			HGameApp.Settings.TaskDurationModifier = durationMod;
			return null;
		}

		#endregion
	}
}