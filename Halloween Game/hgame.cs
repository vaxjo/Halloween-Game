using System;
using Newtonsoft.Json;
using System.Linq;
using System.Data.Linq;
using System.Collections.Generic;
using StackExchange.Profiling;

namespace Halloween_Game {
	public partial class hgameDataContext {
		public static hgameDataContext GetDataContext() {
			var dc = new hgameDataContext();
			return new hgameDataContext(new StackExchange.Profiling.Data.ProfiledDbConnection(dc.Connection, MiniProfiler.Current));
		}
	}
}
