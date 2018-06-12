using System;
using System.Collections.Generic;
using System.Linq;

namespace Halloween_Game {
	public partial class Newsfeed {
		public enum Context { info, success, danger, warning }

		public void Delete() {
			hgameDataContext dc = hgameDataContext.GetDataContext();
			dc.Newsfeeds.DeleteOnSubmit(dc.Newsfeeds.SingleOrDefault(o => o.id == id));
			dc.SubmitChanges();

			Myriads.Cache.Remove("Newsfeeds");
		}

		/// <summary> All Newsfeeds for current session. </summary>
		public static List<Newsfeed> GetAll() {
			return (List<Newsfeed>)Myriads.Cache.Get("Newsfeeds", Session.CurrentSessionId, delegate() {
				hgameDataContext dc = hgameDataContext.GetDataContext();
				return dc.Newsfeeds.Where(o => o.sessionId == Session.CurrentSessionId).ToList();
			});
		}

		/// <summary> Returns most recent if id = null. </summary>
		public static Newsfeed Load(int? id = null) {
			if (id.HasValue) return GetAll().SingleOrDefault(o => o.id == id);

			return GetAll().OrderByDescending(o => o.created).First();
		}

		public static Newsfeed Create(string body, Context context) {
			hgameDataContext dc = hgameDataContext.GetDataContext();
			dc.Newsfeeds.InsertOnSubmit(new Newsfeed() { sessionId = Session.CurrentSessionId, created = DateTime.Now, body = body, context = context.ToString() });
			dc.SubmitChanges();

			Myriads.Cache.Remove("Newsfeeds");

			return Load();
		}


		/// <summary> Periodically add a new newsfeed item. </summary>
		public static void AddNewsfeedItem() {
			Newsfeed mostRecent = Newsfeed.Load();
			if (DateTime.Now.Subtract(mostRecent.created).TotalSeconds > 120) {
				var allitems = System.IO.File.ReadAllLines(System.Web.HttpContext.Current.Server.MapPath("/App_Data/newsitems.txt")).Where(o => !string.IsNullOrWhiteSpace(o) && !o.StartsWith("#")).ToList();

				// remove those that have just been displayed
				var lastFew = GetAll().OrderByDescending(o => o.created).Take(7).Select(n => n.body);
				allitems.RemoveAll(o => lastFew.Contains(o.Split('\t')[1]));

				var item = allitems[HGameApp.Rnd.Next(allitems.Count)];
				Newsfeed.Create(item.Split('\t')[1], (Newsfeed.Context)Enum.Parse(typeof(Newsfeed.Context), item.Split('\t')[0]));
			}
		}
	}
}