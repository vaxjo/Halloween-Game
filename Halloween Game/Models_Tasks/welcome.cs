using System;
using System.Collections.Generic;
using System.Linq;

namespace Halloween_Game {
	public class Task_welcome : TaskDetail {

		public override void UpdateTask(PlayerTask playerTask, System.Web.Mvc.FormCollection form) {
			Player player = Player.Load(playerTask.playerId);
			player.Rank += 9;
			player.Save();

			player.GetTeam.score += 5;
			player.GetTeam.Save();

			playerTask.State = Task.TaskState.Completed;
			playerTask.Save();

			Item startItem = player.GiveRandomItem(1).GetItem;
			Notification.CreatePlayer("Welcome to the <b>" + player.GetTeam.name + "</b> team, agent <b>" + player.Name + "</b>. We've given you this <b>" + startItem.name + "</b> to help get you started.", player.id);
		}

	}
}