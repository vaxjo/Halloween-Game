using System;
using System.Linq;

namespace Halloween_Game {
	public class Task_wakeup : TaskDetail {

		public override void UpdateTask(PlayerTask playerTask, System.Web.Mvc.FormCollection form) {
			Player player = Player.Load(playerTask.playerId);
			player.idle = false;
			player.IdleUpdate = null;
			player.Save(); 
			
			playerTask.State = Task.TaskState.Completed;
			playerTask.Save();
		}
	}
}