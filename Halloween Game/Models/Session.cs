using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.Web;

namespace Halloween_Game {
    public partial class Session {
        public enum SessionState { Running, TimeWarp, Reset }

        public static Guid CurrentSessionId {
            get { return (Guid) HttpContext.Current.Application["gameSessionid"]; }
            set { HttpContext.Current.Application["gameSessionid"] = value; }
        }

        public static int TimeWarpLevel {
            get { return (HttpContext.Current.Application["TimeWarpLevel"] == null ? 0 : (int) HttpContext.Current.Application["TimeWarpLevel"]); }
            set { HttpContext.Current.Application["TimeWarpLevel"] = value; }
        }

        public string Code { get { return id.ToString("N").Substring(0, 4).ToUpper(); } }

        public static Session CurrentSession { get { return Session.Load(CurrentSessionId); } }

        public TimeSpan TimeSinceStart { get { return DateTime.Now.Subtract(start); } }

        public TimeSpan TimeSinceLastModified { get { return DateTime.Now.Subtract(lastModified); } }

        public SessionState State {
            get { return (SessionState) Enum.Parse(typeof(SessionState), state); }
            set { state = value.ToString(); }
        }

        public void SetState(SessionState newState) {
            State = newState;

            hgameDataContext dc = hgameDataContext.GetDataContext();
            var session = dc.Sessions.SingleOrDefault(o => o.id == id);
            session.state = state;
            session.lastModified = DateTime.Now;
            dc.SubmitChanges();

            Myriads.Cache.Remove("Session");
        }

        public void Delete() {
            hgameDataContext dc = hgameDataContext.GetDataContext();
            dc.Sessions.DeleteOnSubmit(dc.Sessions.SingleOrDefault(o => o.id == id));
            dc.SubmitChanges();

            Myriads.Cache.Remove("Session");
        }

        public static void StartNewSession() {
            var previousSessionCodes = GetAll().Select(o => o.Code).ToList();

            // theoretically, this could turn into an infinite loop - but only once there 10,000 sessions have been created
            Session newSession = new Session();
            bool newIdIsOkay = true;
            do {
                newIdIsOkay = true;
                newSession = new Session() { id = Guid.NewGuid(), start = DateTime.Now, state = SessionState.Running.ToString(), lastModified = DateTime.Now };
                // don't re-use session ids
                if (previousSessionCodes.Contains(newSession.Code)) newIdIsOkay = false;
                // no duplicate join codes
                List<string> joinCodes = new List<string>();
                foreach (Team team in Team.GetAll()) joinCodes.Add(team.GetJoinCode(newSession.Code));
                if (joinCodes.Count != joinCodes.Distinct().Count()) newIdIsOkay = false;
            } while (!newIdIsOkay);

            hgameDataContext dc = hgameDataContext.GetDataContext();
            dc.Sessions.InsertOnSubmit(newSession);
            dc.SubmitChanges();

            foreach (Team team in Team.GetAll()) {
                team.score = 0;
                team.Save();
            }

            CurrentSessionId = newSession.id;
            TimeWarpLevel = 0;

            Newsfeed.Create("Strange new alien technology discovered! Who will be the first to develop it?", Newsfeed.Context.success);

            Myriads.Cache.Remove("Session");
        }

        public static List<Session> GetAll() {
            return (List<Session>) Myriads.Cache.Get("Session", "all", delegate () {
                hgameDataContext dc = hgameDataContext.GetDataContext();
                return dc.Sessions.ToList();
            });
        }

        public static Session Load(Guid sessionId) {
            return GetAll().SingleOrDefault(o => o.id == sessionId);
        }
    }
}