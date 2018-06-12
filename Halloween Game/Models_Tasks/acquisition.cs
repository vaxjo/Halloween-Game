using System;
using System.Collections.Generic;
using System.Linq;

namespace Halloween_Game {
	public class Task_acquisition : TaskDetail {

		public override bool CheckDependencies(Player player) {
			// there must be at least one item to get
			return AllOtherTeamItems(player).Count > 0;
		}

		/// <summary> List of items that other people have that you do not that could fit in your available slots. </summary>
		protected List<PlayerItem> AllOtherTeamItems(Player player) {
			List<PlayerItem> allotherTeamItems = player.GetTeam.GetPlayers.SelectMany(o => o.GetItems).Where(o => !player.GetItems.Select(i => i.itemId).Contains(o.itemId)).ToList();

			return allotherTeamItems.Where(o => o.GetItem.size <= player.AvailableItemSlots).ToList();
		}

		public override void UpdateTask(PlayerTask playerTask, System.Web.Mvc.FormCollection form) {
			Player player = Player.Load(playerTask.playerId);

			playerTask.State = Task.TaskState.Completed;
			playerTask.Save();

			PlayerItem acquiredItem = PlayerItem.Load(form["data"]);
			acquiredItem.Delete();
			player.AddItem(acquiredItem.itemId);

			player.Rank += 9;
			player.Save();
		}
	}
}