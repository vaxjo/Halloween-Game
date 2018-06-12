using System;
using System.Collections.Generic;
using System.Linq;

namespace Halloween_Game {
	public class Task_slots : TaskDetail {

		public override string Init(Player player) {
			return "5"; // starting money
		}

		public override void UpdateTask(PlayerTask playerTask, System.Web.Mvc.FormCollection form) {
			Player player = Player.Load(playerTask.playerId);

			if (form["money"] != null) {
				playerTask.data = form["money"];
				playerTask.Save();
				return;
			}

			// if they haven't any slots, then give the team a tech bonus instead
			if (player.AvailableItemSlots == 0) {
				player.GetTeam.score += Convert.ToInt32(form["win"]) * 10;
				player.GetTeam.Save();
				Notification.CreatePlayer("Through shrewd negotiation you manage to convince the space casino to convert your winnings into a research grant for your team.", player.id);
			}

			if (form["win"] == "3") {
				player.GiveRandomItem(Math.Min(3, player.AvailableItemSlots));
				player.Rank += 15;

			} else if (form["win"] == "2") {
				player.GiveRandomItem(Math.Min(2, player.AvailableItemSlots));
				player.Rank += 9;

			} else if (form["win"] == "1") {
				player.GiveRandomItem(Math.Min(1, player.AvailableItemSlots));
				player.Rank += 4;

			} else if (form["win"] == "0") {
				player.Rank -= 9;
			}
			player.Save();

			playerTask.State = Task.TaskState.Completed;
			playerTask.Save(); 
		}
	}
}