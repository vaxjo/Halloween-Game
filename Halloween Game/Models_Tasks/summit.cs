using System;
using System.Collections.Generic;
using System.Linq;

namespace Halloween_Game {
	public class Task_summit : TaskDetail {
		private int[] _primes = new int[] { 1, 2, 3, 5, 7, 11 };

		public override bool CheckDependencies(Player player) {
			// only one at a time
			if (PlayerTask.GetAll().Any(o => o.taskId == "summit" && o.State == Task.TaskState.Waiting)) return false;

			// all teams must have active players
			// AND you must be the mostly highly ranked (or at least tied)
			return (Team.GetAll().All(o => o.GetPlayers.Count > 0) && player.GetTeam.GetPlayers.Max(o => o.Rank) == player.Rank);
		}

		public override string Init(Player player) {
			return _primes[HGameApp.Rnd.Next(_primes.Length)] + ",0"; // my prime,my product guess
		}

		public override void Created(PlayerTask playerTask) {
			// other teams
			foreach (Team team in Team.GetAll().Where(o => o.id != playerTask.GetPlayer.teamId)) {
				var highestRankedAgent = team.GetPlayers.OrderByDescending(o => o.Rank).First();
				if (highestRankedAgent == null) continue;
				PlayerTask.CreateTask("summit", highestRankedAgent.id, _primes[HGameApp.Rnd.Next(_primes.Length)] + ",0");
			}

			Newsfeed.Create("The once-every-century Earth Plutocratic Congress Interagency Technology Summit had commenced. Best of luck to the company representatives.", Newsfeed.Context.success);
		}

		private static List<PlayerTask> GetSummitTasks() {
			return PlayerTask.GetAll().Where(o => o.taskId == "summit").OrderByDescending(o => o.assigned).Take(3).ToList();
		}

		private static int GetProduct() {
			int product = 1;
			foreach (var pt in GetSummitTasks()) product *= Convert.ToInt32(pt.data.Split(',')[0]);
			return product;
		}

		private static bool IsPrimary(PlayerTask playerTask) {
			return (playerTask.id == GetSummitTasks().Min(o => o.id));
		}

		public override string GetStatus(PlayerTask playerTask) {
			if (GetSummitTasks().Any(o => o.data.Split(',')[1] == "0")) return ""; // still waiting

			var allGuesses = GetSummitTasks().Select(o => Convert.ToInt32(o.data.Split(',')[1]));
			if (allGuesses.All(o => o == allGuesses.First()) && allGuesses.First() == GetProduct()) return "True"; // got it all right

			return "False"; // at least one person was wrong
		}

		public override void UpdateTask(PlayerTask playerTask, System.Web.Mvc.FormCollection form) {
			Player player = Player.Load(playerTask.playerId);

			// data submitted; log it and continue to wait
			if (form["data"] != null) {
				playerTask.data = playerTask.data.Split(',')[0] + "," + form["data"];
				playerTask.Save();
				return;
			}

			// all submissions have been made
			string status = GetStatus(playerTask);
			if (status == "") return;

			// personal bonus for getting it right
			if (Convert.ToInt32(playerTask.data.Split(',')[1]) == GetProduct()) {
				player.Rank += 10;
			} else {
				player.Rank -= 10;
				Notification.CreatePlayer(string.Join(" x ", GetSummitTasks().Select(o => o.data.Split(',')[0])) + " = " + GetProduct() + ". Duh.", player.id);
			}
			player.Save();

			if (IsPrimary(playerTask)) {
				if (status == "True") {
					foreach (var pt in GetSummitTasks()) {
						pt.GetPlayer.GetTeam.score += 15;
						pt.GetPlayer.GetTeam.Save();
					}
					Newsfeed.Create("The Earth Plutocratic Congress Interagency Technology Summit is once again a overwhelming success.", Newsfeed.Context.success);

				} else {
					foreach (var pt in GetSummitTasks()) {
						pt.GetPlayer.GetTeam.score -= 8;
						pt.GetPlayer.GetTeam.Save();
					}

					Newsfeed.Create("Tragedy at the Earth Plutocratic Congress Interagency Technology Summit caused by a simple mathematical error.", Newsfeed.Context.warning);
				}
			}

			playerTask.State = Task.TaskState.Completed;
			playerTask.Save();
		}
	}
}