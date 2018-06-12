using System;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.Web;

namespace Halloween_Game {
	public partial class PlayerItem {

		public PlayerItem_JSON GetJSON { get { return new PlayerItem_JSON() { id = id, player = GetPlayer.GetJSON, item = GetItem.GetJSON }; } }

		public Player GetPlayer { get { return Player.Load(playerId); } }

		public Item GetItem { get { return Item.Load(itemId); } }

		public string TransferCode {
			get {
				System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
				byte[] b = md5.ComputeHash(Encoding.UTF8.GetBytes(GetItem.name + id.ToString()));
				string mess = (b[0] * b[1] * b[2]).ToString();
				return mess.PadRight(4, '6').Substring(0, 4);
			}
		}

		public void Save() {
			hgameDataContext dc = hgameDataContext.GetDataContext();

			PlayerItem PlayerItem = dc.PlayerItems.Single(o => o.id == id);
			// we don't actually have any variables here, yet.
			dc.SubmitChanges();

			Myriads.Cache.Remove("PlayerItem");
		}

		public void Delete() {
			hgameDataContext dc = hgameDataContext.GetDataContext();
			dc.PlayerItems.DeleteOnSubmit(dc.PlayerItems.SingleOrDefault(o => o.id == id));
			dc.SubmitChanges();

			Myriads.Cache.Remove("PlayerItem");
		}

		/// <summary> Returns all items for all players (or the specified player) in the current game session. </summary>
		public static List<PlayerItem> GetAll(Guid? playerId = null) {
			List<PlayerItem> all = (List<PlayerItem>)Myriads.Cache.Get("PlayerItem", "all", delegate() {
				hgameDataContext dc = hgameDataContext.GetDataContext();
				return dc.PlayerItems.Where(o => o.Player.sessionId == Session.CurrentSessionId).ToList();
			});

			return all.Where(o => !playerId.HasValue || o.playerId == playerId.Value).ToList();
		}
		
		public static PlayerItem Load(int playerItemId) {
			return GetAll().SingleOrDefault(o => o.id == playerItemId);
		}

		public static PlayerItem Load(string transferCode) {
			return GetAll().SingleOrDefault(o => o.TransferCode == transferCode);
		}

		public static PlayerItem AddItem(string itemId, Guid playerId) {
			hgameDataContext dc = hgameDataContext.GetDataContext();
			dc.PlayerItems.InsertOnSubmit(new PlayerItem() { playerId = playerId, itemId = itemId });
			dc.SubmitChanges();

			Myriads.Cache.Remove("PlayerItem");

			return GetAll().Where(o => o.playerId == playerId && o.itemId == itemId).OrderByDescending(o => o.id).First();
		}
	}
}