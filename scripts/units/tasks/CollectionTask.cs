using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

namespace SacaSimulationGame.scripts.units.tasks
{
    public class CollectionTask: UnitTask
    {
        public CollectionTask(List<UnitTask> tasks)
        {
            foreach (var task in tasks) {
                task.ParentTask = this;
            }
            this.Tasks = tasks;
        }
        public List<UnitTask> Tasks { get; set; }

        public override bool IsFinished => Tasks.Count == 0 || Tasks.All(t => t.IsFinished);

        public override Vector3 TaskPosition => throw new NotImplementedException();
    }
}
