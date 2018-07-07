using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.Web;

namespace Halloween_Game {
	public partial class Player {
		public static Guid? CurrentPlayerId { get { return (Guid?)HttpContext.Current.Session["hgamePlayerId"]; } }
		public static Player CurrentPlayer { get { return CurrentPlayerId.HasValue ? Player.Load(CurrentPlayerId.Value) : null; } }

		public Player_JSON GetJSON { get { return new Player_JSON() { id = id, name = Name, role = Role, team = GetTeam.GetJSON }; } }

		public string ImageUrl { get { return "http://miner.jarrin.net/?batch=FluxCorp&" + id; } }

		public string Name {
			get {
				Random rnd = new Random(id.GetHashCode());

				string subopts = "aaeeeeooooiiiiuuuyyyyyetiaonsetiaonjjjsetiaonsetiaonsrhldcrhldcumfpywgumfpywgbvkxjqzáéíóúýäöüßəþɔŋƿƕƣʔ";
				string namedivs = "   '";
				string name = "";

				int numSubNames = rnd.Next(3) + 1;
				for (int numSubs = 0; numSubs < numSubNames; numSubs++) {
					string subname = "";
					int subnameLength = rnd.Next(5) + 2;
					for (int i = 0; i < subnameLength; i++) {
						char c = subopts[rnd.Next(subopts.Length)];
						subname += c;
					}
					subname = subopts[rnd.Next(subopts.Length)].ToString().ToUpper() + subname;

					if (string.IsNullOrWhiteSpace(name)) {
						name = subname;
					} else {
						name = name + namedivs[rnd.Next(namedivs.Length)] + subname;
					}
				}

				return name;
			}
		}

		public string Role {
			get {
				string[] roles = { "Intern", "Clerk", "Adjunct", "Technician", "Researcher", "Engineer", "Specialist", "Director", "Subcommander", "President" };
				string[] prefixes = { "Provisional", "Assistant", "Associate", "Primary", "Secondary", "Tertiary", "Vice", "Senior", "Chief", "Super" };

				int r = Math.Min(Math.Max(0, (int)Math.Floor((double)Rank / (double)roles.Length)), roles.Length - 1); // [0,99] => [0, 9] (given roles.length = 10)
				int p = Rank - r * roles.Length;

				return prefixes[p] + " " + roles[r];
			}
		}

		/// <summary> 0 to 99 </summary>
		public int Rank {
			get { return rank; }
			set { rank = Math.Min(99, Math.Max(0, value)); }
		}

		/// <summary> Last time this player was updated for any sort of idle state handling. Returns null if not idle. </summary>
		public DateTime? IdleUpdate {
			get { return (idle ? (DateTime?)HttpContext.Current.Application.Get("idle " + id) : null); }
			set { HttpContext.Current.Application.Set("idle " + id, value); }
        }

        public DateTime LastActivity {
            get => (DateTime) HttpContext.Current.Application.Get("LastActivity " + id);
            set => HttpContext.Current.Application.Set("LastActivity " + id, value);
        }

        public DateTime Creation {
            get => (DateTime) HttpContext.Current.Application.Get("Creation " + id);
            set => HttpContext.Current.Application.Set("Creation " + id, value);
        }

        public string AgentCode {
			get {
				if (HttpContext.Current.Application["agent code " + id] == null) GenerateNewAgentCode();
				return (string)HttpContext.Current.Application["agent code " + id];
			}
		}

		// 1 to 5 task slots
		public int TaskSlots {
			get { return Math.Max(1, Rank / 20); }
		}

		public int AvailableItemSlots { get { return 4 - GetItems.Sum(o => o.GetItem.size); } }

		public bool MustReload {
			get { return HttpContext.Current.Application["mustreload_" + id] != null; }
			set {
				if (value) HttpContext.Current.Application.Set("mustreload_" + id, true);
				else HttpContext.Current.Application.Remove("mustreload_" + id);
			}
		}

		public List<int> ReadNotifications;

		public Team GetTeam { get { return Halloween_Game.Team.Load(teamId); } }

		/// <summary> List of tasks waiting to be completed. </summary>
		public List<PlayerTask> GetTasks {
			get {
				return PlayerTask.GetAll(id).Where(o => o.state == Task.TaskState.Waiting.ToString()).ToList();
			}
		}

		public List<PlayerItem> GetItems { get { return PlayerItem.GetAll(id).ToList(); } }

		partial void OnLoaded() {
			ReadNotifications = readNotifications.Split(',').Where(o => !string.IsNullOrWhiteSpace(o)).Select(o => Convert.ToInt32(o)).ToList();
		}

		/// <summary> Whether player has the specified item. </summary>
		public bool HasItem(string itemId) {
			return GetItems.Select(o => o.itemId).Contains(itemId);
		}

		/// <summary> Whether player is currently assigned the specified task. </summary>
		public bool HasTask(string taskId) {
			return GetTasks.Select(o => o.taskId).Contains(taskId);
		}

		public void GenerateNewAgentCode() {
			HttpContext.Current.Application.Set("agent code " + id, HGameApp.Rnd.Next(1000, 10000).ToString());
		}

		/// <summary> Check for and manage idle players. </summary>
		public void CheckIdle() {
			// already idle, see about transfering possessions
			if (idle) {
				if (!IdleUpdate.HasValue) IdleUpdate = DateTime.Now;
				if (DateTime.Now.Subtract(IdleUpdate.Value).TotalMinutes < 2) return; // wait two minutes between reappropriation
				IdleUpdate = DateTime.Now;

				// give away player's items
				var otherplayers = GetTeam.GetPlayers.Where(o => o.id != id);
				foreach (PlayerItem item in GetItems.OrderByDescending(o => o.GetItem.size)) {
					var possibleRecipient = otherplayers.Where(o => o.AvailableItemSlots >= item.GetItem.size).ToList();
					if (possibleRecipient.Count == 0) continue; // sadly, no one can take this item

					var recipient = possibleRecipient[HGameApp.Rnd.Next(possibleRecipient.Count)];
					recipient.AddItem(item.itemId);
					Notification.CreatePlayer("You notice your teammate, <b>" + Name + "</b>, taking a nap so you appropriate the unused <b>" + item.GetItem.name + "</b>.", recipient.id);
					item.Delete();
					Notification.CreatePlayer("While you were napping, another agent borrowed your <b>" + item.GetItem.name + "</b>.", id);
					return;
				}

				return;
			}

			if (PlayerTask.GetAll(id).Count < 3 || GetTasks.Count > 0) return; // must have had at least three task and cannot have any current active tasks

			if (!PlayerTask.GetAll(id).OrderByDescending(o => o.assigned).Take(3).All(o => o.State == Task.TaskState.Expired)) return; // 3 most-recent must all be expired

			idle = true;
			Save();

			Notification.CreatePlayer("You've apparently decided to take a nap for some reason.", id);
			IdleUpdate = DateTime.Now;

			PlayerTask.CreateTask("wakeup", id);
		}

		public void RemoveItem(string itemId) {
			var item = GetItems.FirstOrDefault(o => o.itemId == itemId);
			if (item != null) item.Delete();
		}

		public PlayerItem AddItem(string itemId) {
			Item addItem = Item.Load(itemId);

			List<string> warehousedItems = new List<string>();
			while (AvailableItemSlots < addItem.size) {
				Item smallestItem = GetItems.OrderBy(o => o.GetItem.size).First().GetItem;
				RemoveItem(smallestItem.id);
				GetTeam.WarehouseAddItem(smallestItem.id);
				warehousedItems.Add(smallestItem.name);
			}

			if (warehousedItems.Count > 0) {
				Notification.CreatePlayer("Your new <b>" + addItem.name + "</b> was too large to fit in your inventory, so your <b>" + string.Join(", ", warehousedItems) + "</b> " + (warehousedItems.Count > 1 ? "have" : "has") + " been moved to the company warehouse.", id);
			}

			return PlayerItem.AddItem(itemId, id);
		}

		/// <summary> Give a random item (of specified size) to player (if possible). </summary>
		public PlayerItem GiveRandomItem(int itemSize) {
			if (AvailableItemSlots < itemSize) return null; // no room

			Item item = Item.GetRandomItem(itemSize);
			return (item != null ? AddItem(item.id) : null);
		}

		public void Save() {
			hgameDataContext dc = hgameDataContext.GetDataContext();

			Player player = dc.Players.SingleOrDefault(o => o.id == id);
			if (player == null) {
				player = new Player() { id = id, created = DateTime.Now };
				dc.Players.InsertOnSubmit(player);
			}

			player.teamId = teamId;
			player.sessionId = sessionId;
			player.rank = rank;
			player.idle = idle;
			player.readNotifications = string.Join(",", ReadNotifications);
			dc.SubmitChanges();

            LastActivity = DateTime.Now;

            Myriads.Cache.Remove("Player");
		}

		public void Delete() {
			hgameDataContext dc = hgameDataContext.GetDataContext();
			dc.Players.DeleteOnSubmit(dc.Players.SingleOrDefault(o => o.id == id));
			dc.SubmitChanges();

			Myriads.Cache.Remove("Player");
		}

		public static List<Player> GetAll(bool allSessions = false) {
			List<Player> all = (List<Player>)Myriads.Cache.Get("Player", "all", delegate() {
				hgameDataContext dc = hgameDataContext.GetDataContext();
				return dc.Players.ToList();
			});

			return all.Where(o => allSessions || o.sessionId == Session.CurrentSessionId).ToList();
		}

		public static Player Load(Guid id) {
			return GetAll().SingleOrDefault(o => o.id == id);
		}

		public static Player Load(string agentCode) {
			return GetAll().SingleOrDefault(o => o.AgentCode == agentCode);
		}

		public static Player Create(Team team) {
			Guid id = Guid.NewGuid();

			hgameDataContext dc = hgameDataContext.GetDataContext();
			dc.Players.InsertOnSubmit(new Player() { id = id, teamId = team.id, sessionId = Session.CurrentSessionId, created = DateTime.Now, readNotifications = "" });
			dc.SubmitChanges();

			Myriads.Cache.Remove("Player");

			Player p = Player.Load(id);
            p.Creation = DateTime.Now;
            p.LastActivity = DateTime.Now;

            Newsfeed.Create("A new agent, <b>" + p.Name + "</b>,  has joined <b>" + p.GetTeam.name + "</b>.", Newsfeed.Context.info);

			Notification.CreateTeam("<b>" + p.Name + "</b> has joined your team.", p.teamId);

			p.ReadNotifications = Notification.GetAllForTeam(team.id).Select(o => o.id).ToList();
			p.Save();

			return p;
		}

		public override string ToString() {
			return Name;
		}
	}

}