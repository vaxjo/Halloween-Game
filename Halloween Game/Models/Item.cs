using System;
using System.Collections.Generic;
using System.Linq;

namespace Halloween_Game {
	public partial class Item {
		public Item_JSON GetJSON { get { return new Item_JSON() { id = id, name = name, size = size }; } }

		public void Save() {
			hgameDataContext dc = hgameDataContext.GetDataContext();

			Item item = dc.Items.SingleOrDefault(o => o.id == id);
			if (item == null) {
				item = new Item() { id = id };
				dc.Items.InsertOnSubmit(item);
			}

			item.name = name;
			item.description = description;
			item.size = size;
			item.unique = unique;
			dc.SubmitChanges();

			Myriads.Cache.Remove("Item");
		}

		public void Delete() {
			hgameDataContext dc = hgameDataContext.GetDataContext();
			dc.Items.DeleteOnSubmit(dc.Items.SingleOrDefault(o => o.id == id));
			dc.SubmitChanges();

			Myriads.Cache.Remove("Item");
			Myriads.Cache.Remove("PlayerItem");
		}

		public static List<Item> GetAll() {
			return (List<Item>)Myriads.Cache.Get("Item", "all", delegate() {
				hgameDataContext dc = hgameDataContext.GetDataContext();
				return dc.Items.ToList();
			});
		}

		public static Item Load(string itemId) {
			return GetAll().SingleOrDefault(o => o.id == itemId);
		}

		/// <summary> Get a non-unique random item of specified size. </summary>
		public static Item GetRandomItem(int itemSize) {
			if (itemSize < 1 || itemSize > 4) return null;

			var possibleItems = Item.GetAll().Where(o => o.size == itemSize && !o.unique).ToList();
			return possibleItems[HGameApp.Rnd.Next(possibleItems.Count)];
		}

		public static string Combine(int item1Id, int item2Id) {

			// combined items in alphabetical order (by item id)
			List<PlayerItem> combinedItems = new List<PlayerItem> { PlayerItem.Load(item1Id), PlayerItem.Load(item2Id) };
			combinedItems = combinedItems.OrderBy(o => o.itemId).ToList();
			Player player = combinedItems[0].GetPlayer;
			if (combinedItems[0].playerId != combinedItems[1].playerId) return null; // this should never happen - can only combine items from a single player's inventory

			// the ids of the items in alphabetical order
			string bothItems = string.Join(",", combinedItems.Select(o => o.itemId));

            // 27b/6
            if (combinedItems.Any(o => o.itemId == "27b6")) {
                PlayerItem t27b6Item = combinedItems.First(o => o.itemId == "27b6");
                PlayerItem otherItem = combinedItems.Where(o => o.id != t27b6Item.id).First();
                return "You apply the <b>" + t27b6Item.GetItem.name + "</b> to the <b>" + otherItem.GetItem.name + "</b> but realize that the <b>" + t27b6Item.GetItem.name + "</b> doesn't have the necessary stamp from Information Retreival.";
            }
            
            // acid destroys the both things
            if (combinedItems.Any(o => o.itemId == "acid")) {
				PlayerItem acidItem = combinedItems.First(o => o.itemId == "acid");
				PlayerItem otherItem = combinedItems.Where(o => o.id != acidItem.id).First(); // could also be acid!

				// both acid! let's make a thing!
				if (otherItem.itemId == "acid") {
					acidItem.Delete();
					otherItem.Delete();
					PlayerItem sentienceItem = player.AddItem("sentience");
					return "You pour the two <b>" + acidItem.GetItem.name + "s</b> together and, after a violent conflagration of acrid smoke, discover that you've created <b>" + sentienceItem.GetItem.name + "</b>.";
				}

				// not unique items
				if (otherItem.GetItem.unique) {
					acidItem.Delete();
					return "The <b>" + acidItem.GetItem.name + "</b> drips off the <b>" + otherItem.GetItem.name + "</b> leaving it completely unmarred. Fascinating.";
				}

				// dissolves whatever
				acidItem.Delete();
				otherItem.Delete();
				return "You immerse the <b>" + otherItem.GetItem.name + "</b> in the <b>" + acidItem.GetItem.name + "</b> and it completely dissolves.";
			}

            if (bothItems == "coffee,coffee") {
				combinedItems[0].Delete();
                return "You combine the two cups of coffee into one. Brilliant!";
            }

            // normal research tree construction
            if (bothItems == "electronics,sentience") {
				if (player.GetTeam.TechLevel < 1) return InsufficientTech(combinedItems);
				combinedItems[0].Delete();
				combinedItems[1].Delete();
				PlayerItem newItem = player.AddItem("stabilizer");
				return "The <b>" + combinedItems[1].GetItem.name + "</b> fuses with some of the <b>" + combinedItems[0].GetItem.name + "</b> and creates an <b>" + newItem.GetItem.name + "</b>.";
			}

			if (bothItems == "electronics,essence") {
				if (player.GetTeam.TechLevel < 1) return InsufficientTech(combinedItems);
				combinedItems[0].Delete();
				combinedItems[1].Delete();
				PlayerItem newItem = player.AddItem("chronotons");
				return "The <b>" + combinedItems[1].GetItem.name + "</b> absorbs some of the <b>" + combinedItems[0].GetItem.name + "</b> and creates a <b>" + newItem.GetItem.name + "</b>.";
			}

			if (bothItems == "electronics,photonic") {
				if (player.GetTeam.TechLevel < 1) return InsufficientTech(combinedItems);
				combinedItems[0].Delete();
				combinedItems[1].Delete();
				PlayerItem newItem = player.AddItem("fluxcapacitor");
				return "You enhance the <b>" + combinedItems[1].GetItem.name + "</b> with some <b>" + combinedItems[0].GetItem.name + "</b> and assemble a <b>" + newItem.GetItem.name + "</b>.";
			}

			if (bothItems == "essence,photonic") {
				if (player.GetTeam.TechLevel < 1) return InsufficientTech(combinedItems);
				combinedItems[0].Delete();
				combinedItems[1].Delete();
				PlayerItem newItem = player.AddItem("separator");
				return "You route the output of the <b>" + combinedItems[1].GetItem.name + "</b> through the <b>" + combinedItems[0].GetItem.name + "</b> and devise a make-shift <b>" + newItem.GetItem.name + "</b>.";
			}

			// second level research
			if (bothItems == "albumen,stabilizer") {
				if (player.GetTeam.TechLevel < 2) return InsufficientTech(combinedItems);
				combinedItems[0].Delete();
				combinedItems[1].Delete();
				PlayerItem newItem = player.AddItem("converter");
				Newsfeed.Create("<b>" + player.GetTeam.name + "</b> makes great strides in developing the strange alien technology.", Newsfeed.Context.success);
				return "You inject the <b>" + combinedItems[0].GetItem.name + "</b> into the <b>" + combinedItems[1].GetItem.name + "</b> and invent an <b>" + newItem.GetItem.name + "</b>.";
			}

			if (bothItems == "albumen,chronotons") {
				if (player.GetTeam.TechLevel < 2) return InsufficientTech(combinedItems);
				combinedItems[0].Delete();
				combinedItems[1].Delete();
				PlayerItem newItem = player.AddItem("manifold");
				Newsfeed.Create("<b>" + player.GetTeam.name + "</b> continues to uncover the secrets of the strange alien technology.", Newsfeed.Context.success);
				return "You integrate the <b>" + combinedItems[0].GetItem.name + "</b> with the <b>" + combinedItems[1].GetItem.name + "</b> and realize you now have a <b>" + newItem.GetItem.name + "</b>.";
			}

			if (bothItems == "fluxcapacitor,separator") {
				if (player.GetTeam.TechLevel < 2) return InsufficientTech(combinedItems);
				combinedItems[0].Delete();
				combinedItems[1].Delete();
				PlayerItem newItem = player.AddItem("compensator");
				Newsfeed.Create("<b>" + player.GetTeam.name + "</b> publishes ground-breaking paper detailing the strange alien technology.", Newsfeed.Context.success);
				return "You hurridly tape the <b>" + combinedItems[0].GetItem.name + "</b> to the front of the <b>" + combinedItems[1].GetItem.name + "</b> because you need a <b>" + newItem.GetItem.name + "</b>.";
			}

            // easier combinations [jj 18Jul6]
            if (bothItems == "photonic,stabilizer") {
                if (player.GetTeam.TechLevel < 2) return InsufficientTech(combinedItems);
                combinedItems[0].Delete();
                combinedItems[1].Delete();
                PlayerItem newItem = player.AddItem("converter");
                return "You connect the fleshy output port of the <b>" + combinedItems[1].GetItem.name + "</b> to the <b>" + combinedItems[0].GetItem.name + "</b> and fashion a sort-of <b>" + newItem.GetItem.name + "</b>.";
            }

            if (bothItems == "chronotons,sentience") {
                if (player.GetTeam.TechLevel < 2) return InsufficientTech(combinedItems);
                combinedItems[0].Delete();
                combinedItems[1].Delete();
                PlayerItem newItem = player.AddItem("manifold");
                return "You bathe the <b>" + combinedItems[0].GetItem.name + "</b> in a quantity of <b>" + combinedItems[1].GetItem.name + "</b> and realize that it's basically a <b>" + newItem.GetItem.name + "</b>.";
            }

            if (bothItems == "electronics,separator") {
                if (player.GetTeam.TechLevel < 2) return InsufficientTech(combinedItems);
                combinedItems[0].Delete();
                combinedItems[1].Delete();
                PlayerItem newItem = player.AddItem("compensator");
                return "Using the <b>" + combinedItems[0].GetItem.name + "</b> to slightly adjust the <b>" + combinedItems[1].GetItem.name + "</b> you find yourself with a serviceable <b>" + newItem.GetItem.name + "</b>.";
            }

            // generic nothing event
            return "You tried to combine <b>" + combinedItems[0].GetItem.name + "</b> with <b>" + combinedItems[1].GetItem.name + "</b>, but nothing happened.";
		}

		public static string InsufficientTech(List<PlayerItem> combinedItems) {
			return "You tried to combine <b>" + combinedItems[0].GetItem.name + "</b> with <b>" + combinedItems[1].GetItem.name + "</b>, which seems like it should have worked, but <b>" + combinedItems[0].GetPlayer.GetTeam.name + "'s</b> tech level isn't high enough.";
		}
	}
}