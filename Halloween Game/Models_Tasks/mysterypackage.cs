using System;
using System.Collections.Generic;
using System.Linq;

namespace Halloween_Game {
	public class Task_mysterypackage : TaskDetail {

		public override bool CheckDependencies(Player player) {
			return player.AvailableItemSlots >= 1;
		}

		public override string Init(Player player) {
			// all unique items currently held by players on your team
			List<PlayerItem> allUniqueTeamItems = player.GetTeam.GetPlayers.SelectMany(o => o.GetItems).Where(o => o.GetItem.unique).ToList();

			// distinct list of those item ids
			List<string> uniqueItems = allUniqueTeamItems.Select(o => o.itemId).Distinct().ToList();

			var allSmallItems = Item.GetAll().Where(o => o.size == 1).Where(o => !uniqueItems.Contains(o.id)).ToList();
			return allSmallItems[HGameApp.Rnd.Next(allSmallItems.Count)].id;
		}

		public override void UpdateTask(PlayerTask playerTask, System.Web.Mvc.FormCollection form) {
			Player player = Player.Load(playerTask.playerId);

			playerTask.State = Task.TaskState.Completed;
			playerTask.Save();

			player.AddItem(playerTask.data);
		}
	}
}