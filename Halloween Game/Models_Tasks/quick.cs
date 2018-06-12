using System;
using System.Linq;

namespace Halloween_Game {
	public class Task_quick : TaskDetail {

		public override void UpdateTask(PlayerTask playerTask, System.Web.Mvc.FormCollection form) {
			playerTask.State = Task.TaskState.Completed;
			playerTask.Save();

			Player player = Player.Load(playerTask.playerId);
			player.Rank+=5;
			player.Save();
		}
	}
}