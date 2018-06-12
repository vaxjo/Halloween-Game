using System;
using System.Collections.Generic;
using System.Linq;

namespace Halloween_Game {
	public class Task_trading : TaskDetail {

		public override bool CheckDependencies(Player player) {
			// there must be at least one item to get
			return AllOtherTeamItems(player).Count > 0;
		}

		public override string Init(Player player) {
			// find an item that another player on your team has that you do not
			List<PlayerItem> allotherTeamItems = player.GetTeam.GetPlayers.SelectMany(o => o.GetItems).Where(o => !player.GetItems.Select(i => i.itemId).Contains(o.itemId)).ToList();

			return allotherTeamItems[HGameApp.Rnd.Next(allotherTeamItems.Count)].id.ToString();
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

			if (form["data"] == PlayerItem.Load(Convert.ToInt32(playerTask.data)).TransferCode) {
				player.Rank+=13;
				player.Save();

				PlayerItem itemToRetrieve = PlayerItem.Load(Convert.ToInt32(playerTask.data));
				player.AddItem(itemToRetrieve.itemId);
				itemToRetrieve.Delete();
			}
		}
	}
}