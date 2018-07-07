using System;
using System.IO;
using System.Web;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;

namespace Halloween_Game {
	public class Task_wiring : TaskDetail {
		private static DirectoryInfo _wiringDir = new DirectoryInfo(HttpContext.Current.Server.MapPath("~/Content/Tasks/wiring"));

		public override string Init(Player player) {
			int x = 0, y = 0;
			do {
				x = HGameApp.Rnd.Next(2, 6);
				y = HGameApp.Rnd.Next(2, 6);
			} while (x * y > (player.Rank / 8) + 9); // (4,6,9,12,16,20,25) , 9 - 21.5

			FileInfo[] allImages = _wiringDir.GetFiles(); // they should all be jpgs

			return Path.GetFileNameWithoutExtension(allImages[HGameApp.Rnd.Next(allImages.Length)].Name) + "," + x + "," + y;  // "filename,xres,yres"
		}

		public override void Created(PlayerTask playerTask) {
			string filename = playerTask.data.Split(',')[0];
			int xres = Convert.ToInt32(playerTask.data.Split(',')[1]);
			int yres = Convert.ToInt32(playerTask.data.Split(',')[2]);

			DirectoryInfo wiringResDir = new DirectoryInfo(HttpContext.Current.Server.MapPath("~/Content/Tasks/wiring/" + filename + "_" + xres + "x" + yres));
			if (!wiringResDir.Exists) wiringResDir.Create();

			using (
			Bitmap source = (Bitmap)Bitmap.FromFile(Path.Combine(_wiringDir.FullName, filename + ".jpg"))) {
				int xSize = source.Width / xres, ySize = source.Height / yres;

				for (int x = 0; x < xres; x++)
					for (int y = 0; y < yres; y++) {
						using (Bitmap slice = new Bitmap(source, xSize, ySize)) {
							Graphics g = Graphics.FromImage(slice);
							g.DrawImage(source, new Rectangle(0, 0, xSize, ySize), x * xSize, y * ySize, xSize, ySize, GraphicsUnit.Pixel, new System.Drawing.Imaging.ImageAttributes());

							slice.Save(Path.Combine(wiringResDir.FullName, x + "x" + y + ".jpg")); // wiring/a_3x3/0_1.jpg
						}
					}
			}
		}

		public override void UpdateTask(PlayerTask playerTask, System.Web.Mvc.FormCollection form) {
			Player player = Player.Load(playerTask.playerId);

			int xres = Convert.ToInt32(playerTask.data.Split(',')[1]);
			int yres = Convert.ToInt32(playerTask.data.Split(',')[2]);

			player.Rank += (xres * yres);
			player.Save();

			if (player.AvailableItemSlots >= 1) player.GiveRandomItem(1);

			playerTask.State = Task.TaskState.Completed;
			playerTask.Save();
		}
	}
}