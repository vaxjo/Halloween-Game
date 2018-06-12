using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Halloween_Game.Controllers {
	public class ItemController : Controller {
		public ActionResult Combine(int item1Id, int item2Id) {
			return Content(Item.Combine(item1Id, item2Id));
		}

		public JsonResult GetPlayerItemStatus(string transferCode) {
			PlayerItem item = PlayerItem.Load(transferCode);
			if (item == null) return Json(new PlayerItem_JSON() { id = 0 }, JsonRequestBehavior.AllowGet);

			return Json(item.GetJSON, JsonRequestBehavior.AllowGet);
		}

		public EmptyResult DoNotPress(int id) {
			PlayerItem item = PlayerItem.Load(id);
			if (item.itemId != "timemachine") return null; // hax0rs
			
			// put session into TimeWarp state - central display takes care of the flow from here on out
			Halloween_Game.Session.CurrentSession.SetState(Halloween_Game.Session.SessionState.TimeWarp);
			System.Web.HttpContext.Current.Application["TimeWarp PlayerItem Id"] = item.id;

			return null;
		}

		[HGameAuth]
		public EmptyResult Drop(int id) {
			PlayerItem item = PlayerItem.Load(id);
			item.Delete();
			return null;
		}
	}
}