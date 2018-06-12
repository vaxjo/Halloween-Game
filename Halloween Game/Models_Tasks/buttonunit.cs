using System;
using System.Collections.Generic;
using System.Linq;

namespace Halloween_Game {
	public class Task_buttonunit : TaskDetail {

		public override bool CheckDependencies(Player player) {
			// task can't already exist
			if (PlayerTask.GetAll().Any(o => o.State == Task.TaskState.Waiting && o.taskId == "buttonunit")) return false;

			// must have a slot
			return (player.AvailableItemSlots > 0);
		}

		public override string Init(Player player) {
			// figrue out who had the button unit last (if any)
			var lastBUtask = PlayerTask.GetAll().Where(o => o.taskId == "buttonunit").OrderByDescending(o => o.assigned).FirstOrDefault();

			return (lastBUtask != null ? lastBUtask.playerId.ToString() : ""); // player id of the last person to have the button unit
		}

		public override void UpdateTask(PlayerTask playerTask, System.Web.Mvc.FormCollection form) {
			Player player = Player.Load(playerTask.playerId);

			// all players on other teams with items (non-unique, 2 or smaller)
			int maxSize = Math.Max(2, player.AvailableItemSlots);
			var playersWithItems = Team.GetAll().Where(o => o.id != playerTask.GetPlayer.teamId).SelectMany(o => o.GetPlayers).Where(o => o.GetItems.Count(i => i.GetItem.size <= maxSize && !i.GetItem.unique) > 0).ToList();

			if (player.AvailableItemSlots < 1) {
				// do nothing, I guess

			} else if (playersWithItems.Count == 0) {
				// no one to steal from, so just give them an item
				player.GiveRandomItem(1);

			} else {
				Player victim = playersWithItems[HGameApp.Rnd.Next(playersWithItems.Count)];
				var possibleItems = victim.GetItems.Where(o => o.GetItem.size <= maxSize).ToList();
				var victimItem = possibleItems[HGameApp.Rnd.Next(possibleItems.Count)];

				player.AddItem(victimItem.itemId);

				victimItem.Delete();
				Notification.CreatePlayer("Your <b>" + victimItem.GetItem.name + "</b> dissappears under mysterious circumstances.", victim.id);

				// 20% of the time just stop
				if (HGameApp.Rnd.Next(100) > 20) PlayerTask.CreateTask("buttonunit", victim.id);
			}

			Notification.CreatePlayer("The strange man returns, reclaims the Button Unit, and explains that it will be offered to someone whom you don't know.", player.id);

			playerTask.State = Task.TaskState.Completed;
			playerTask.Save();
		}

		public override void Expired(PlayerTask playerTask) {
			playerTask.GetPlayer.Rank += 21;
			playerTask.GetPlayer.Save();
		}
	}
}