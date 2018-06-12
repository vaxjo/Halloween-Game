using System;
using System.Collections.Generic;
using System.Linq;

namespace Halloween_Game {
	public class Task_prisonersdilemma : TaskDetail {

		public override bool CheckDependencies(Player player) {
			// there must be at least one other player on another team
			return (Team.GetAll().Where(o => o.id != player.teamId).SelectMany(o => o.GetPlayers).Count() > 0);
		}

		public override string Init(Player player) {
			// find a suitable opponent
			var otherPlayers = Team.GetAll().Where(o => o.id != player.teamId).SelectMany(o => o.GetPlayers).ToList();
			Guid opponentId = otherPlayers[HGameApp.Rnd.Next(otherPlayers.Count)].id;
			return opponentId.ToString(); // opponent id - this is temporary and will be replaced in Created()
		}

		public override void Created(PlayerTask playerTask) {
			Guid opponentId = new Guid(playerTask.data); // take the opponent id from the temp data value

			// by specifying the data param we avoid unwanted recursion in player task creation
			PlayerTask oppoTask = PlayerTask.CreateTask("prisonersdilemma", opponentId, playerTask.id + ",");

			playerTask.data = oppoTask.id + ","; // from now on, the data values will be: "{opponent task id},{my chosen state}"
			playerTask.Save();
		}

		public override string GetStatus(PlayerTask playerTask) {
			PlayerTask opponentTask = PlayerTask.Load(Convert.ToInt32(playerTask.data.Split(',')[0]));
			return opponentTask.data.Split(',')[1];  // all we need is the state of the opponent's choice
		}

		public override void UpdateTask(PlayerTask playerTask, System.Web.Mvc.FormCollection form) {
			Player player = Player.Load(playerTask.playerId);
			int opponentTaskId = Convert.ToInt32(playerTask.data.Split(',')[0]);

			// player has made their choice
			if (form["choice"] != null) {
				playerTask.data = opponentTaskId + "," + form["choice"];
				playerTask.Save();
				return;
			}

			// both players have made their choices
			PlayerTask opponentTask = PlayerTask.Load(opponentTaskId);

			string myChoice = playerTask.data.Split(',')[1];
			string opponentChoice = opponentTask.data.Split(',')[1];

			if (myChoice == "coop" && opponentChoice == "coop") {
				Notification.CreatePlayer("Both you and <b>" + opponentTask.GetPlayer.Name + "</b> cooperated with one another and have received your just rewards.", player.id);
				player.Rank += 12;
				player.Save();

				player.GetTeam.score += 13;
				player.GetTeam.Save();

				player.GiveRandomItem(Math.Min(player.AvailableItemSlots, 2));

			} else if (myChoice == "betray" && opponentChoice == "coop") {
				Notification.CreatePlayer("You betrayed <b>" + opponentTask.GetPlayer.Name + "</b> while they tried to cooperate with you.", player.id);
				player.GiveRandomItem(Math.Min(player.AvailableItemSlots, 1));

			} else if (myChoice == "coop" && opponentChoice == "betray") {
				Notification.CreatePlayer("You were betrayed by <b>" + opponentTask.GetPlayer.Name + "</b>.", player.id);
				player.Rank -= 10;
				player.Save();

			} else { // betray/betray
				Notification.CreatePlayer("You both betrayed one another.", player.id);
				player.Rank += 10;
				player.Save();
			}

			playerTask.State = Task.TaskState.Completed;
			playerTask.Save();
		}
	}
}