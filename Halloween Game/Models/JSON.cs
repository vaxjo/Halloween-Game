using System;
using System.Collections.Generic;
using System.Linq;

namespace Halloween_Game {
	public class Task_JSON {
		public string id;
		public string name;
	}

	public class Team_JSON {
		public string id;
		public string name;
	}

	public class Player_JSON {
		public Team_JSON team;
		public Guid id;
		public string name, role;
	}

	public class Notification_JSON {
		public int id;
		public string message;
		public int age; // ms
	}

	public class Item_JSON {
		public string id, name;
		public int size;
	}

	public class PlayerItem_JSON {
		public Item_JSON item;
		public Player_JSON player;
		public int id;
	}

	public class PlayerTask_JSON {
		public Task_JSON task;
		public Player_JSON player;
		public int id;
		public string data, status;
	}
}
