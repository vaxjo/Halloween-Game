using System;
using System.Linq;

namespace Halloween_Game {
	/// <summary> Details for a specific task. </summary>
	public abstract class TaskDetail {

		/// <summary> True if this task can be assigned to the specified player.</summary>
		public virtual bool CheckDependencies(Player player) {
			return true;
		}

		/// <summary> Called once when the task is assigned to the player. </summary>
		public virtual string Init(Player player) {
			return "";
		}

		/// <summary> Occurs immediately after a task has been created and assigned to a player. This is a good spot to generate more/related tasks (I hope). </summary>
		public virtual void Created(PlayerTask playerTask) {
			return;
		}

		/// <summary> Available at any time to get the "status" of a task. </summary>
		public virtual string GetStatus(PlayerTask playerTask) {
			return "";
		}

		/// <summary> After the player has interacted with the task's form, the form data is sent here for processing. </summary>
		public abstract void UpdateTask(PlayerTask playerTask, System.Web.Mvc.FormCollection form);

		/// <summary> Called if this task exipres. In case we want something special to happen. </summary>
		public virtual void Expired(PlayerTask playerTask) {
			return;
		}
	}
}