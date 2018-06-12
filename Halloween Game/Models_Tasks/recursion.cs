using System;
using System.Collections.Generic;
using System.Linq;

namespace Halloween_Game {
	public class Task_recursion : TaskDetail {

		public override void UpdateTask(PlayerTask playerTask, System.Web.Mvc.FormCollection form) {
			Player player = Player.Load(playerTask.playerId);

			int recursionLevel = Convert.ToInt32(form["recursionLevel"]);
			if (recursionLevel == 0) return; // not enough recursion

			if (recursionLevel < 3) {
				player.Rank += recursionLevel;
				player.Save();
				Notification.CreatePlayer("That really wasn't that much recursion.", player.id);

			} else if (recursionLevel < 6) {
				player.Rank += recursionLevel;
				player.Save();
				player.GetTeam.score += recursionLevel;
				player.GetTeam.Save();
				Notification.CreatePlayer("That was a reasonable amount of recursion.", player.id);
			
			} else if (recursionLevel < 9) {
				player.Rank += recursionLevel;
				player.Save();
				player.GetTeam.score += recursionLevel;
				player.GetTeam.Save();
				Notification.CreatePlayer("That was rather a lot of recursion.", player.id);
			
			} else {
				player.Rank += 10;
				player.Save();
				player.GetTeam.score += 10;
				player.GetTeam.Save(); 
				Notification.CreatePlayer("That was an absurd amount of recursion.", player.id);
			}

			playerTask.State = Task.TaskState.Completed;
			playerTask.Save();
		}
	}
}