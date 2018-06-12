using System;
using System.Collections.Generic;
using System.Linq;

namespace Halloween_Game {
	public class Task_warehouse : TaskDetail {

		public override bool CheckDependencies(Player player) {
			return player.GetTeam.WarehouseGet().Any(o => o.size <= player.AvailableItemSlots);
		}

		public override void UpdateTask(PlayerTask playerTask, System.Web.Mvc.FormCollection form) {
			Player player = Player.Load(playerTask.playerId);

			if (form["itemId"] != null) {
				Item item = player.GetTeam.WarehouseRetrieveItem(form["itemId"]);
				player.AddItem(item.id);

				player.Rank += 9;
				player.Save();
			}

			playerTask.State = Task.TaskState.Completed;
			playerTask.Save();
		}
	}
}