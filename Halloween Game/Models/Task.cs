using System;
using System.Collections.Generic;
using System.Linq;

namespace Halloween_Game {
    public partial class Task {
        public enum TaskState { Completed, Failed, Waiting, Expired }

        public Task_JSON GetJSON { get { return new Task_JSON() { id = id, name = name }; } }

        /// <summary> Duration until task expires, modified by global duration setting. </summary>
        public TimeSpan Duration {
            get { return TimeSpan.FromSeconds((double) duration * HGameApp.Settings.TaskDurationModifier / 100.0); }
            set { duration = (int) value.TotalSeconds; }
        }

        public TaskDetail GetTaskDetail() {
            //TaskDetail taskDetail = (TaskDetail)Activator.CreateInstance(Type.GetType("Halloween_Game.Task_" + id), new object[] { pt });
            TaskDetail taskDetail = (TaskDetail) Activator.CreateInstance(Type.GetType("Halloween_Game.Task_" + id));
            return taskDetail;
        }

        public void Save() {
            hgameDataContext dc = hgameDataContext.GetDataContext();

            Task task = dc.Tasks.SingleOrDefault(o => o.id == id);
            if (task == null) {
                task = new Task() { id = id };
                dc.Tasks.InsertOnSubmit(task);
            }

            task.active = active;
            task.manual = manual;
            task.name = name;
            task.duration = duration;
            task.minPlayerRank = minPlayerRank;
            task.minTechLevel = minTechLevel;
            dc.SubmitChanges();

            Myriads.Cache.Remove("Task");
        }

        public void Delete() {
            hgameDataContext dc = hgameDataContext.GetDataContext();
            dc.Tasks.DeleteOnSubmit(dc.Tasks.SingleOrDefault(o => o.id == id));
            dc.SubmitChanges();

            Myriads.Cache.Remove("Task");
            Myriads.Cache.Remove("PlayerTask");
        }

        public static List<Task> GetAll() {
            return (List<Task>) Myriads.Cache.Get("Task", "all", delegate () {
                hgameDataContext dc = hgameDataContext.GetDataContext();
                return dc.Tasks.ToList();
            });
        }

        public static Task Load(string taskId) {
            return GetAll().SingleOrDefault(o => o.id == taskId);
        }

        public override string ToString() => "[" + id + "] " + name;
    }
}