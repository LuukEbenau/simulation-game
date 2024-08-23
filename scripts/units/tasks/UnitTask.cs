using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading.Tasks;
using Godot;
using SacaSimulationGame.scripts.naturalResources;

namespace SacaSimulationGame.scripts.units.tasks
{
    /// <summary>
    /// A unit task is a scheduled task to be picked up by a unit
    /// </summary>
    public abstract class UnitTask
    {
        //Types of tasks: Gather stone, gather wood,
        // Deliver resources to building?
        // IDEA: make everything a tasks, so we can chain tasks together. Some tasks have as prerequirement other tasks, and create new tasks afterwards.
        public abstract bool IsFinished { get; }
        public List<UnitTask> FollowUpTasks { get; set; } = new();

        public abstract Vector3 TaskPosition { get; }

        public UnitTask ParentTask { get; set; }
    }
}
