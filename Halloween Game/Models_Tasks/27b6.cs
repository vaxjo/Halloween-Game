using System;
using System.Collections.Generic;
using System.Linq;

namespace Halloween_Game {
	public class Task_27b6 : TaskDetail {

		public override bool CheckDependencies(Player player) {
			if (player.HasItem("27b6")) return false; // can't get it from yourself

			// all instances of 27b/6 belonging to other people on your team
			List<PlayerItem> allOther27b6Items = player.GetTeam.GetPlayers.Where(o => o.id != player.id).SelectMany(o => o.GetItems).Where(o => o.GetItem.id == "27b6").ToList();

			// coarsely delete any extras (there should be only one!)
			foreach (PlayerItem item in allOther27b6Items.Skip(1)) item.Delete();

			// there must be at least one item to get
			return allOther27b6Items.Count > 0;
		}

		public override string Init(Player player) {
			return player.GetTeam.GetPlayers.Where(o => o.id != player.id).SelectMany(o => o.GetItems).Where(o => o.GetItem.id == "27b6").First().id.ToString();
		}

		public override void UpdateTask(PlayerTask playerTask, System.Web.Mvc.FormCollection form) {
			Player player = Player.Load(playerTask.playerId);

			playerTask.State = Task.TaskState.Completed;
			playerTask.Save();

			if (form["data"] == PlayerItem.Load(Convert.ToInt32(playerTask.data)).TransferCode) {
				player.Rank += 7;
				player.Save();

				PlayerItem itemToRetrieve = PlayerItem.Load(Convert.ToInt32(playerTask.data));
				player.AddItem(itemToRetrieve.itemId);
				itemToRetrieve.Delete();
			}
		}
	}
}