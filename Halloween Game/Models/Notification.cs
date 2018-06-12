using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.Web;

namespace Halloween_Game {
	public partial class Notification {

		public Notification_JSON GetJSON { get { return new Notification_JSON() { id = id, message = message, age = (int)DateTime.Now.Subtract(created).TotalMilliseconds }; } }

		public void Delete() {
			hgameDataContext dc = hgameDataContext.GetDataContext();
			dc.Notifications.DeleteOnSubmit(dc.Notifications.SingleOrDefault(o => o.id == id));
			dc.SubmitChanges();

			Myriads.Cache.Remove("Notifications");
		}

		/// <summary> All notifications for current session, regardless of recipient. </summary>
		public static List<Notification> GetAll() {
			return (List<Notification>)Myriads.Cache.Get("Notifications", Session.CurrentSessionId, delegate() {
				hgameDataContext dc = hgameDataContext.GetDataContext();
				return dc.Notifications.Where(o => o.sessionId == Session.CurrentSessionId).ToList();
			});
		}

		public static List<Notification> GetAllForPlayer(Guid playerId) {
			Player player = Player.Load(playerId);
			// where global OR player is specified OR player's team is specified
			return GetAll().Where(o => (string.IsNullOrWhiteSpace(o.recipientTeamId) && !o.recipientPlayerId.HasValue) || o.recipientPlayerId == playerId || o.recipientTeamId == player.teamId).ToList();
		}

		public static List<Notification> GetAllForTeam(string teamId) {
			// where global OR player's team is specified
			return GetAll().Where(o => (string.IsNullOrWhiteSpace(o.recipientTeamId) && !o.recipientPlayerId.HasValue) || o.recipientTeamId == teamId).ToList();
		}

		/// <summary> Returns most recent if id = null. </summary>
		public static Notification Load(int? id = null) {
			if (id.HasValue) return GetAll().SingleOrDefault(o => o.id == id);

			return GetAll().OrderByDescending(o => o.created).First();
		}

		public static Notification CreateGlobal(string message) {
			return Create(message, null, null);
		}

		public static Notification CreateTeam(string message, string teamId) {
			return Create(message, teamId, null);
		}

		public static Notification CreatePlayer(string message, Guid playerId) {
			return Create(message, null, playerId);
		}

		public static Notification Create(string message, string teamId = null, Guid? playerId = null) {
			hgameDataContext dc = hgameDataContext.GetDataContext();
			dc.Notifications.InsertOnSubmit(new Notification() { sessionId = Session.CurrentSessionId, created = DateTime.Now, message = message, recipientTeamId = teamId, recipientPlayerId = playerId });
			dc.SubmitChanges();

			Myriads.Cache.Remove("Notifications");

			return Load();
		}
	}
}