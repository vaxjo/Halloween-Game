using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Halloween_Game {
	public class Task_trivia : TaskDetail {
		public static FileInfo TriviaFile = new FileInfo(System.Web.HttpContext.Current.Server.MapPath("/App_Data/trivia.txt"));

		public override bool CheckDependencies(Player player) {
			return TriviaFile.Exists;
		}

		public override string Init(Player player) {
			string[] lines = File.ReadAllLines(TriviaFile.FullName);
			List<string> teamTrivia = lines.Where(o => o.StartsWith(player.teamId)).ToList();	//ccc,2112,blah blah blah

			return teamTrivia[HGameApp.Rnd.Next(teamTrivia.Count)].Substring(4); // 2112,whatevers
		}

		public override void UpdateTask(PlayerTask playerTask, System.Web.Mvc.FormCollection form) {
			playerTask.State = Task.TaskState.Completed;
			playerTask.Save();

			if (form["data"] == playerTask.data.Substring(0,4)) {
				Player player = Player.Load(playerTask.playerId);
				player.Rank += 13;
				player.Save();
			}
		}
	}
}