using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.Web;

namespace Halloween_Game {
	public class Settings {
		public static FileInfo SettingsFile { get { return new FileInfo(HttpContext.Current.Server.MapPath("/App_Data/settings.json")); } }

		private bool _devMode;
		public bool DevMode {
			get { return _devMode; }
			set { _devMode = value; Save(); }
		}

		private double _taskRate;
		public double TaskRate {
			get { return _taskRate; }
			set { _taskRate = value; Save(); }
		}

		private int _techRate;
		public int TechRate {
			get { return _techRate; }
			set { _techRate = value; Save(); }
		}

		private double _durationMod;
		public double TaskDurationModifier {
			get { return _durationMod; }
			set { _durationMod = value; Save(); }
		}

		internal Settings() {
			_taskRate = 100;
			_techRate = 33;
			_devMode = false;
			_durationMod = 100;
		}

		public void Save() {
			File.WriteAllText(SettingsFile.FullName, Newtonsoft.Json.JsonConvert.SerializeObject(this));
			Myriads.Cache.Remove("Settings");
		}

		public static Settings Load() {
			return (Settings)Myriads.Cache.Get("Settings", "", delegate {
				Settings s = new Settings();
				if (SettingsFile.Exists) s = Newtonsoft.Json.JsonConvert.DeserializeObject<Settings>(File.ReadAllText(SettingsFile.FullName));
				return s;
			});
		}
	}
}