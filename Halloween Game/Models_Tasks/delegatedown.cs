using System;
using System.Collections.Generic;
using System.Linq;

namespace Halloween_Game {
	public class Task_delegatedown : TaskDetail {
	
		public override void UpdateTask(PlayerTask playerTask, System.Web.Mvc.FormCollection form) {
			Player player = Player.Load(playerTask.playerId);

			if (player.GetTeam.GetPlayers.Min(o => o.Rank) == player.Rank) {
				// lowest rank just does it
				player.Rank += 9;
				player.Save();

				player.GetTeam.score += 8;
				player.GetTeam.Save();

				playerTask.State = Task.TaskState.Completed;
				playerTask.Save(); 
				
				return;
			}

			Player transferPlayer = Player.Load(form["data"]);
			if (transferPlayer == null || transferPlayer.Rank > player.Rank) return;

			player.Rank += 8;
			player.Save();

			PlayerTask.CreateTask("delegatedown", transferPlayer.id);
			transferPlayer.GenerateNewAgentCode();

			player.GetTeam.score += 5;
			player.GetTeam.Save();

			playerTask.State = Task.TaskState.Completed;
			playerTask.Save();
		}

		public override void Expired(PlayerTask playerTask) {
			// all lower-ranked teammates get a penalty when  this task expires
			int rankOfTaskPlayer = playerTask.GetPlayer.Rank;
			foreach (Player p in playerTask.GetPlayer.GetTeam.GetPlayers.Where(o => o.Rank < rankOfTaskPlayer)){
				p.Rank -= 10;
				p.Save();
			}
		}
	}
}