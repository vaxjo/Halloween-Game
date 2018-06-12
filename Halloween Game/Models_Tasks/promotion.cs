using System;
using System.Collections.Generic;
using System.Linq;

namespace Halloween_Game {
	public class Task_promotion : TaskDetail {

		public override bool CheckDependencies(Player player) {
			return player.Rank < 60;
		}

		public override void UpdateTask(PlayerTask playerTask, System.Web.Mvc.FormCollection form) {
			Player player = Player.Load(playerTask.playerId);

			playerTask.State = Task.TaskState.Completed;
			playerTask.Save();

			player.Rank += 19;
			player.Save();
		}
	}
}