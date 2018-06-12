using System;
using System.Collections.Generic;
using System.Linq;

namespace Halloween_Game {
	public class Task_marketplace : TaskDetail {

		public override bool CheckDependencies(Player player) {
			// you must have at least one item (sizes 1 or 2)
			return player.GetItems.Any(o => o.GetItem.size < 3);
		}

		public override string Init(Player player) {
			int largestSize = player.GetItems.OrderByDescending(o => o.GetItem.size).First().GetItem.size;

			Item item = Item.GetRandomItem(largestSize);
			return item.id.ToString();
		}

		public override void UpdateTask(PlayerTask playerTask, System.Web.Mvc.FormCollection form) {
			Player player = Player.Load(playerTask.playerId);

			playerTask.State = Task.TaskState.Completed;
			playerTask.Save();

			player.Rank += 6;
			player.Save();

			// action: "trade", itemId: $(".modal #itemId").val() 
			if (form["action"] == "trade") {
				PlayerItem tradedItem = PlayerItem.Load(Convert.ToInt32(form["itemId"]));
				tradedItem.Delete();

				player.AddItem(playerTask.data);
			}
		}
	}
}