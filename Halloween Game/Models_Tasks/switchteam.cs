using System;
using System.Data.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Halloween_Game {
	public class Task_switchteam : TaskDetail {

		protected Team SmallestOtherTeam(Player player) {
			return Team.GetAll().Where(o => o.id != player.GetTeam.id).OrderBy(o => o.GetPlayers.Count).First();
		}

		public override bool CheckDependencies(Player player) {
			// the size difference between the smallest other team and the player's team must be at least 2
			return (player.GetTeam.GetPlayers.Count - SmallestOtherTeam(player).GetPlayers.Count >= 2);
		}

		public override string Init(Player player) {
			return SmallestOtherTeam(player).id;
		}

		public override void UpdateTask(PlayerTask playerTask, System.Web.Mvc.FormCollection form) {
			Player player = Player.Load(playerTask.playerId);

			playerTask.State = Task.TaskState.Completed;
			playerTask.Save();

			if (form["decision"] == "switch") {
				player.Team = Team.Load(playerTask.data);
				player.MustReload = true;
				player.GetTeam.score += 10;
				Notification n = Notification.CreateTeam("<b>" + player.Name + "</b> has switched teams and joined <b>" + player.GetTeam.name + "</b>.", player.GetTeam.id);
				player.ReadNotifications.Add(n.id);
				foreach (PlayerTask task in player.GetTasks) task.Delete(); // delete all your waiting tasks
				Notification.CreatePlayer("Welcome to <b>" + player.GetTeam.name + "</b>.", player.id);


			} else { // stayed with current team
				player.Rank += 8;
				Notification.CreatePlayer("<b>"+ player.GetTeam.name +"</b> appreciates your continued and compulsory loyalty.", player.id);
			}

			player.Save();
		}
	}
}
