using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.Web;

namespace Halloween_Game {
	public partial class Team {
		protected string[] _greekLetters = new string[] { "Alpha", "Beta", "Gamma", "Delta", "Epsilon", "Zeta", "Eta", "Theta", "Iota", "Kappa", "Lambda", "Mu", "Nu", "Xi" };

		protected string _warehouseKey { get { return "item warehouse " + id; } }

		public Team_JSON GetJSON { get { return new Team_JSON() { id = id, name = name }; } }

		public int TechLevel { get { return score / HGameApp.Settings.TechRate; } }

		public string TechLevelLetter { get { return _greekLetters[Math.Min(_greekLetters.Length - 1, TechLevel)]; } }

		/// <summary> Environment-specific url for joining. "/Join/{CODE}?team=ccc" or "http://ccc.jarrin.net/Join/{CODE}". </summary>
		public string JoinUrl { get { return (HttpContext.Current.Request.IsLocal ? "/Join/" + Halloween_Game.Session.CurrentSession.Code + "?team=" + id : "http://" + id + ".jarrin.net/Join/" + Halloween_Game.Session.CurrentSession.Code); } }

		/// <summary> All non-idle players on this team. </summary>
		public List<Player> GetPlayers { get { return Player.GetAll().Where(o => o.teamId == id && !o.idle).ToList(); } }

		/// <summary> Add an item to this team's warehouse. </summary>
		public void WarehouseAddItem(string itemId) {
			List<string> wh = (HttpContext.Current.Application[_warehouseKey] != null ? (List<string>)HttpContext.Current.Application[_warehouseKey] : new List<string>());
			wh.Add(itemId);
			HttpContext.Current.Application.Set(_warehouseKey, wh);
		}

		/// <summary> True if this item is in the warehouse. </summary>
		public bool WarehouseHasItem(string itemId) {
			List<string> wh = (HttpContext.Current.Application[_warehouseKey] != null ? (List<string>)HttpContext.Current.Application[_warehouseKey] : new List<string>());
			return wh.Contains(itemId);
		}

		/// <summary> Remove and return an item from this team's warehouse. </summary>
		public Item WarehouseRetrieveItem(string itemId) {
			List<string> wh = (HttpContext.Current.Application[_warehouseKey] != null ? (List<string>)HttpContext.Current.Application[_warehouseKey] : new List<string>());
			if (!WarehouseHasItem(itemId)) return null;

			wh.Remove(itemId);
			HttpContext.Current.Application.Set(_warehouseKey, wh);

			return Item.Load(itemId);
		}

		/// <summary> List of everything in the team warehouse. </summary>
		public List<Item> WarehouseGet() {
			List<string> wh = (HttpContext.Current.Application[_warehouseKey] != null ? (List<string>)HttpContext.Current.Application[_warehouseKey] : new List<string>());
			return wh.Select(o => Item.Load(o)).ToList();
		}

		/// <summary> Ensure that there exists an n-slot item for teams of tech level n in the warehouse. </summary>
		public void StockWarehouse() {
			int largestWarehouseItemSize = WarehouseGet().Count > 0 ? WarehouseGet().Max(o => o.size) : 0;
			if (largestWarehouseItemSize >= Math.Min(TechLevel, 3)) return;

			var possibleItems = Item.GetAll().Where(o => o.size == Math.Min(TechLevel, 3)).ToList();
			WarehouseAddItem(possibleItems[HGameApp.Rnd.Next(possibleItems.Count)].id);
		}

		public void Save() {
			hgameDataContext dc = hgameDataContext.GetDataContext();

			Team team = dc.Teams.SingleOrDefault(o => o.id == id);
			if (team == null) {
				team = new Team() { id = id };
				dc.Teams.InsertOnSubmit(team);
			}

			team.name = name;
			team.score = score;
			team.description = description;
			team.goal = goal;
			dc.SubmitChanges();

			Myriads.Cache.Remove("Team");
		}

		public void Delete() {
			hgameDataContext dc = hgameDataContext.GetDataContext();
			dc.Teams.DeleteOnSubmit(dc.Teams.SingleOrDefault(o => o.id.ToLower() == id.ToLower()));
			dc.SubmitChanges();

			Myriads.Cache.Remove("Team");
		}

		public static List<Team> GetAll() {
			return (List<Team>)Myriads.Cache.Get("Team", "all", delegate() {
				hgameDataContext dc = hgameDataContext.GetDataContext();
				return dc.Teams.ToList();
			});
		}

		public static Team Load(string teamId) {
			return GetAll().SingleOrDefault(o => o.id.ToLower() == teamId.ToLower());
		}
	}

}