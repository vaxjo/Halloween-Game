using System;
using System.Collections.Generic;
using System.Linq;

namespace Halloween_Game {
	public class Task_coffee : TaskDetail {

		public override bool CheckDependencies(Player player) {
			return player.HasItem("coffee");
		}

		public override void UpdateTask(PlayerTask playerTask, System.Web.Mvc.FormCollection form) {
			Player player = Player.Load(playerTask.playerId);

			if (player.GetTeam.GetPlayers.Max(o => o.Rank) == player.Rank) {
				// highest ranked just drinks it
				player.RemoveItem("coffee");
				player.Rank += 7;
				player.Save();

				player.GetTeam.score += 16;
				player.GetTeam.Save();

				playerTask.State = Task.TaskState.Completed;
				playerTask.Save(); 
				
				return;
			}

			Player transferPlayer = Player.Load(form["data"]);
			if (transferPlayer == null || transferPlayer.Rank < player.Rank || transferPlayer.teamId != player.teamId) return;

			player.RemoveItem("coffee");
			player.Rank += 7;
			player.Save();

			transferPlayer.AddItem("coffee");
			transferPlayer.Rank += 3;
			transferPlayer.Save();
			transferPlayer.GenerateNewAgentCode();

			player.GetTeam.score += 8;
			player.GetTeam.Save();

			playerTask.State = Task.TaskState.Completed;
			playerTask.Save();
		}
	}
}