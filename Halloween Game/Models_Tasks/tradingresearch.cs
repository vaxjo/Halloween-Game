using System;
using System.Linq;

namespace Halloween_Game {
	public class Task_tradingresearch : TaskDetail {

		public override string Init(Player player) {
			string[] commodities = new string[] { "Smithore", "Crystite", "Carbonite", "Dilithium", "Spice", "Adamantine", "Aether", "Byzanium", "Cobalt Thorium G", "Duranium", "Frinkonium", "Ice-nine", "Kryptonite", "Mithril", "Octiron", "Latinum", "Unobtanium", "Verterium Cortenide", "Inaprovaline", "Corbomite " };
			int price = HGameApp.Rnd.Next(100, 1000);
			string commodity = commodities[HGameApp.Rnd.Next(commodities.Length)];

			Newsfeed.Create("Current trading price of " + commodity + ": $" + price, Newsfeed.Context.success);

			return commodity + "," + price;
		}

		public override void UpdateTask(PlayerTask playerTask, System.Web.Mvc.FormCollection form) {
			if (form["data"] != playerTask.data.Split(',')[1]) return;

			Player player = Player.Load(playerTask.playerId);
			player.Rank += 12;
			player.Save();

			playerTask.State = Task.TaskState.Completed;
			playerTask.Save();
		}
	}
}
