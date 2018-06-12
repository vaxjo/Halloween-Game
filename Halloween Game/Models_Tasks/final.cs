using System;
using System.Collections.Generic;
using System.Linq;

namespace Halloween_Game {
	public class Task_final : TaskDetail {

		public override bool CheckDependencies(Player player) {
			// only one at a time
			if (player.GetTeam.GetPlayers.SelectMany(o => o.GetTasks).Any(o => o.taskId == "final")) return false;

			// this agent must have a 3-space item (those are all final assemblies)
			PlayerItem finalAssemblyItem = player.GetItems.SingleOrDefault(o => o.GetItem.size == 3);
			if (finalAssemblyItem == null) return false;

			// all three final assemblies must exist on player's team
			List<string> allTeamItems = player.GetTeam.GetPlayers.SelectMany(o => o.GetItems).Select(o => o.itemId).ToList();
			if (!allTeamItems.Contains("converter") || !allTeamItems.Contains("manifold") || !allTeamItems.Contains("compensator")) return false;

			return true;
		}

		public override string Init(Player player) {
			PlayerItem finalAssemblyItem = player.GetItems.SingleOrDefault(o => o.GetItem.size == 3);
			return finalAssemblyItem.itemId + ",0"; // itemId,device start time (ticks)
		}

		public override void Created(PlayerTask playerTask) {
			PlayerItem finalAssemblyItem = playerTask.GetPlayer.GetItems.SingleOrDefault(o => o.GetItem.size == 3);

			// other final assembly items on your team (excluding yours) // assign them all this task
			foreach (PlayerItem item in playerTask.GetPlayer.GetTeam.GetPlayers.SelectMany(o => o.GetItems).Where(o => o.GetItem.size == 3).Where(o => o.itemId != finalAssemblyItem.itemId)) {
				// if they do not already have this task, then assign it to them
				if (!item.GetPlayer.GetTasks.Any(o => o.taskId == "final")) PlayerTask.CreateTask("final", item.playerId);
			}
		}

		public override string GetStatus(PlayerTask playerTask) {
			// find other tasks, return status of each 
			var allFinalTasks = GetAllFinalTasks(playerTask.GetPlayer);
			string taskStatuses = string.Join(";", allFinalTasks.Select(o => o.data)); // "converter,x;manifold,x;compensator,x"

			// find the timing delay for each task - if any exceeds the threshhold, set timingFailed=true
			var delays = allFinalTasks.Select(o => Convert.ToInt64(o.data.Split(',')[1])).Select(o => new TimeSpan(o == 0 ? 0 : DateTime.Now.Ticks - o));

			string state = "Wait"; // not failed; or not yet begun; or within threshhold
			if (delays.Any(o => o.TotalSeconds > 3.5)) state = "Fail"; // threshhold exceeded
			else if (delays.All(o => o.TotalSeconds > 0)) state = "Done"; // all buttons pressed, not failed, so done!

			return state + ";" + taskStatuses; // "Wait;converter,x;manifold,x;compensator,x"
		}

		public List<PlayerTask> GetAllFinalTasks(Player player) {
			return player.GetTeam.GetPlayers.SelectMany(o => o.GetTasks).Where(o => o.taskId == "final").ToList();
		}

		/// <summary> This playertask is the primary if it has the lowest id of all related tasks. </summary>
		public bool IsPrimaryTask(PlayerTask playerTask) {
			var allFinal = GetAllFinalTasks(playerTask.GetPlayer);
			if (allFinal.Count == 0) return false; // ??? not sure why this would occur
			return allFinal.Select(o => o.id).OrderBy(o => o).First() == playerTask.id;
		}

		public override void UpdateTask(PlayerTask playerTask, System.Web.Mvc.FormCollection form) {
			Player player = Player.Load(playerTask.playerId);

			// player has started their device
			if (form["start"] != null) {
				playerTask.data = form["start"] + "," + DateTime.Now.Ticks;
				playerTask.Save();
				return;
			}

			// timing failed!
			if (GetStatus(playerTask).StartsWith("Fail")) {
				// only do these things once
				if (IsPrimaryTask(playerTask)) {
					Notification.CreateTeam("An explosion rocks the primary laboratory! Your team's lead researchers have failed to properly assembly the final construction. Minor damage results.", player.teamId);
					player.GetTeam.score -= 20; // drop a tech level, but the items are not destroyed
					player.GetTeam.Save();
				}

				playerTask.State = Task.TaskState.Completed;
				playerTask.Save();
				return;
			}

			// success
			if (GetStatus(playerTask).StartsWith("Done")) {
				// only do these things once
				if (IsPrimaryTask(playerTask)) {
					Notification.CreateTeam("A stillness descends over the primary laboratory. Your team's lead researchers have assembled the final form of the strange alien technology and made it available to all agents.", player.teamId);
					// delete everyone's items and give them all the final form
					foreach (Player p in player.GetTeam.GetPlayers) {
						foreach (PlayerItem item in p.GetItems) item.Delete();
						p.AddItem("timemachine");
					}
				}

				playerTask.State = Task.TaskState.Completed;
				playerTask.Save();
				return;
			}
		}
	}
}