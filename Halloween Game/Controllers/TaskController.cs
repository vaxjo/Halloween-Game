using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Halloween_Game.Controllers {
	public class TaskController : Controller {
		public ActionResult ModalBody(int playerTaskId) {
			if (!Player.CurrentPlayerId.HasValue) return Content("");

			PlayerTask pt = PlayerTask.Load(playerTaskId);
			if (pt.playerId != Player.CurrentPlayerId) return Content("ERRRRRROR"); // task doesn't match current player - cheating?

			return View(pt.Task.id, pt); // name.cshtml
		}

		public ActionResult Update(int playerTaskId, FormCollection form) {
			PlayerTask pt = PlayerTask.Load(playerTaskId);
			TaskDetail task = pt.GetTask.GetTaskDetail();
			task.UpdateTask(pt, form);

			return null;
		}

		[HGameAuth]
		public ActionResult Cancel(int id) {
			PlayerTask pt = PlayerTask.Load(id);
			pt.State = Task.TaskState.Completed;
			pt.Save();
			return null;
		}

		public JsonResult GetPlayerTaskStatus(int id) {
			PlayerTask task = PlayerTask.Load(id);
			if (task == null) return Json(new PlayerTask_JSON() { id = 0 }, JsonRequestBehavior.AllowGet);

			return Json(task.GetJSON, JsonRequestBehavior.AllowGet);
		}
	}
}