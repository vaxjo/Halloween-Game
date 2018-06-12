using System;
using System.Collections.Generic;
using System.Linq;

namespace Halloween_Game {
	public class Task_mentoring : TaskDetail {

		public override bool CheckDependencies(Player player) {
			// must have a 3-slot item
			// must exist another teammate with 2-slot available
			return player.GetItems.Any(o => o.GetItem.size == 3) && player.GetTeam.GetPlayers.Any(o => o.AvailableItemSlots >= 2);
		}

		public override string Init(Player player) {
			// teammates with at least 2 item slots available
			var possibles = player.GetTeam.GetPlayers.Where(o => o.AvailableItemSlots >= 2).ToList();
			return possibles[HGameApp.Rnd.Next(possibles.Count)].id.ToString(); // target player id
		}

		public override void Created(PlayerTask playerTask) {
			Notification.CreatePlayer("Another agent of the <b>" + playerTask.GetPlayer.GetTeam.name + "</b> is looking for you to provide some much-needed mentorship.", new Guid(playerTask.data));
		}

		public override void UpdateTask(PlayerTask playerTask, System.Web.Mvc.FormCollection form) {
			Player player = Player.Load(playerTask.playerId);
			player.Rank += 12;
			player.Save();

			Player targetPlayer = Player.Load(new Guid(playerTask.data));
			Item item = Item.Load(form["itemId"]);
			targetPlayer.AddItem(item.id);
			targetPlayer.Rank += 10;
			targetPlayer.Save();

			playerTask.State = Task.TaskState.Completed;
			playerTask.Save();
		}
	}
}