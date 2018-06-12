using System;
using System.Linq;

namespace Halloween_Game {
	public class Task_hatch : TaskDetail {

		public override bool CheckDependencies(Player player) {
			return player.HasItem("egg") && player.AvailableItemSlots >= 1;
		}

		public override void UpdateTask(PlayerTask playerTask, System.Web.Mvc.FormCollection form) {
			Player player = Player.Load(playerTask.playerId);
			player.Rank += 12;
			player.Save();

			player.GetTeam.score += 9;
			player.GetTeam.Save();

			playerTask.State = Task.TaskState.Completed;
			playerTask.Save();

			player.RemoveItem("egg");
			player.AddItem("albumen");
		}
	}
}