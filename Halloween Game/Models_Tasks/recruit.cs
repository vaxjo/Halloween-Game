using System;
using System.Collections.Generic;
using System.Linq;

namespace Halloween_Game {
    public class Task_recruit : TaskDetail {

        public override bool CheckDependencies(Player player) {
            return true;
        }

        public override string Init(Player player) {
            return DateTime.Now.ToString(); // record the start time of this task
        }

        public override string GetStatus(PlayerTask playerTask) {
            // if a new player has joined this team since this task started, we're good
            DateTime taskStart = DateTime.Parse(playerTask.data);

            foreach (Player teammate in playerTask.Player.Team.GetPlayers) {
                if (teammate.Creation > taskStart) return "True";
            }

            // it hasn't happened yet
            return "";
        }

        public override void UpdateTask(PlayerTask playerTask, System.Web.Mvc.FormCollection form) {
            Player player = Player.Load(playerTask.playerId);
            player.Rank += 11;
            player.Save();

            playerTask.State = Task.TaskState.Completed;
            playerTask.Save();
        }
    }
}