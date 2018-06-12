using System;
using System.Collections.Generic;
using System.Linq;

namespace Halloween_Game {
	public class Task_recommendation : TaskDetail {

		public override bool CheckDependencies(Player player) {
			return (player.GetTeam.GetPlayers.Count > 1); // must be at least one other player on your team
		}

		public override string GetStatus(PlayerTask playerTask) {
			Player p = Player.Load(playerTask.data);
			return (p != null && p.id != playerTask.playerId).ToString();
		}

		public override void UpdateTask(PlayerTask playerTask, System.Web.Mvc.FormCollection form) {
			Player player = Player.Load(playerTask.playerId);

			if (form["check"] != null) {
				playerTask.data = form["check"];
				playerTask.Save();
				return;
			}

			if (form["data"] == null) return;

			Player recommendedPlayer = Player.Load(form["data"]);
			if (recommendedPlayer == null) return;

			recommendedPlayer.Rank += 15;
			recommendedPlayer.Save();
			Notification.CreatePlayer("The <b>" + player.GetTeam.name + "</b> is happy to present you with this coveted Award of Recognition and a commensurate increase in your role within the company. Finally.", recommendedPlayer.id);
			recommendedPlayer.GenerateNewAgentCode();

			player.Rank += 7;
			player.Save();

			playerTask.State = Task.TaskState.Completed;
			playerTask.Save();

			Newsfeed.Create("<b>" + player.GetTeam.name + "</b> has issued an Award of Recognition to <b>" + recommendedPlayer.Name + "</b>. Thanks for all your hard work!", Newsfeed.Context.success);
		}
	}
}