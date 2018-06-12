using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.Web;

namespace Halloween_Game {
	public partial class PlayerTask {

		public PlayerTask_JSON GetJSON { get { return new PlayerTask_JSON() { id = id, data = data, status = GetTask.GetTaskDetail().GetStatus(this), player = GetPlayer.GetJSON, task = GetTask.GetJSON }; } }

		public Player GetPlayer { get { return Player.Load(playerId); } }

		public Task GetTask { get { return Task.Load(taskId); } }

		public DateTime Expiration { get { return assigned.Add(GetTask.Duration); } }

		public Task.TaskState State {
			get { return (Task.TaskState)Enum.Parse(typeof(Task.TaskState), state); }
			set { state = value.ToString(); }
		}

		public void Save() {
			hgameDataContext dc = hgameDataContext.GetDataContext();

			PlayerTask PlayerTask = dc.PlayerTasks.Single(o => o.id == id);
			PlayerTask.state = state;
			PlayerTask.data = data;
			dc.SubmitChanges();

			Myriads.Cache.Remove("PlayerTask");
		}

		public void Delete() {
			hgameDataContext dc = hgameDataContext.GetDataContext();
			dc.PlayerTasks.DeleteOnSubmit(dc.PlayerTasks.SingleOrDefault(o => o.id == id));
			dc.SubmitChanges();

			Myriads.Cache.Remove("PlayerTask");
		}

		/// <summary> Returns all tasks for all players (or the specified player) in the current game session, regardless of task state. </summary>
		public static List<PlayerTask> GetAll(Guid? playerId = null) {
			List<PlayerTask> all = (List<PlayerTask>)Myriads.Cache.Get("PlayerTask", "all", delegate() {
				hgameDataContext dc = hgameDataContext.GetDataContext();
				return dc.PlayerTasks.Where(o => o.Player.sessionId == Session.CurrentSessionId).ToList();
			});

			return all.Where(o => !playerId.HasValue || o.playerId == playerId.Value).ToList();
		}

		public static PlayerTask Load(int playerTaskId) {
			return GetAll().SingleOrDefault(o => o.id == playerTaskId);
		}

		public static PlayerTask GetLastTask(Guid playerId) {
			return GetAll(playerId).OrderByDescending(o => o.assigned).FirstOrDefault();
		}

		/// <summary> Mark any "Waiting" tasks as "Expired" if they are. </summary>
		public static void ExpireTasks() {
			List<PlayerTask> expiredTasks = GetAll().Where(o => o.State == Task.TaskState.Waiting && o.Expiration < DateTime.Now).ToList();
			if (expiredTasks.Count == 0) return;

			// in additional to the normal expiration functionality, there may be some special tasks
			foreach (PlayerTask pt in expiredTasks) pt.GetTask.GetTaskDetail().Expired(pt);
			
			hgameDataContext dc = hgameDataContext.GetDataContext();
			foreach (PlayerTask playerTaskDb in dc.PlayerTasks.Where(o => expiredTasks.Select(p => p.id).Contains(o.id))) {
				playerTaskDb.State = Task.TaskState.Expired;
				Notification.Create("Your task, '" + playerTaskDb.GetTask.name + "', has expired.", null, playerTaskDb.playerId);
				
				// every expired task results in a loss of rank
				Player player = Player.Load(playerTaskDb.playerId);
				player.Rank -= 9;
				player.Save();
			}
			dc.SubmitChanges();
			
			Myriads.Cache.Remove("PlayerTask");
		}

		/// <summary> Assign a new task to this player. Maybe. </summary>
		public static void AssignTaskMaybe(Guid playerId) {
			Player player = Player.Load(playerId);

			player.CheckIdle();

			// no slots for new tasks
			if (GetAll(playerId).Where(o => o.State == Task.TaskState.Waiting).Count() >= player.TaskSlots) return;

			var lastTask = GetLastTask(playerId);
			TimeSpan timeSinceLastTask = lastTask != null ? DateTime.Now.Subtract(lastTask.assigned) : TimeSpan.MaxValue;

			// the larger the timeSince value is, the more likely we are to get a new task
			double p = timeSinceLastTask.TotalSeconds * HGameApp.Settings.TaskRate / 100;
			if (HGameApp.Rnd.Next(100) > p) return;

			// find all suitable tasks
			var allSuitableTasks = Task.GetAll().Where(o => o.active && !o.manual && player.Rank >= o.minPlayerRank && player.GetTeam.TechLevel >= o.minTechLevel && o.GetTaskDetail().CheckDependencies(player)).ToList();
			
			// remove any tasks that the player has just had (within the past five)
			var lastFiveTasks = PlayerTask.GetAll(playerId).OrderByDescending(o => o.assigned).Take(5);
			allSuitableTasks.RemoveAll(o => lastFiveTasks.Select(t => t.taskId).Contains(o.id));
			
			if (allSuitableTasks.Count == 0) return;

			// assign this player a new task
			Task newTask = allSuitableTasks[HGameApp.Rnd.Next(allSuitableTasks.Count)];
			CreateTask(newTask.id, playerId);
		}

		/// <summary> Assign this task to the specified player, ignoring dependencies and slots. If you specify a data string neither Init() nor Created() will be called on this player task. </summary>
		public static PlayerTask CreateTask(string taskId, Guid playerId, string data = null) {
			Task task = Task.Load(taskId);

			try {
				hgameDataContext dc = hgameDataContext.GetDataContext();
				dc.PlayerTasks.InsertOnSubmit(new PlayerTask() {
					playerId = playerId, taskId = taskId, state = Task.TaskState.Waiting.ToString(), assigned = DateTime.Now,
					data = (data == null ? task.GetTaskDetail().Init(Player.Load(playerId)) : data)
				});
				dc.SubmitChanges();

				Myriads.Cache.Remove("PlayerTask");

				PlayerTask lastTask = GetLastTask(playerId);
				if (data == null) lastTask.GetTask.GetTaskDetail().Created(lastTask);
				return lastTask;

			} catch { } // sometimes things don't work out

			return null;
		}

	}
}