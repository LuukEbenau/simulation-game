using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using SacaSimulationGame.scripts.buildings;

namespace SacaSimulationGame.scripts.units.tasks
{
    public class DropOffResourcesAtDepositTask : UnitTask
    {
        /// <summary>
        /// This is a unit specific task, dont add to task queue
        /// </summary>
        /// <param name="building"></param>
        /// <param name="unit"></param>
        public DropOffResourcesAtDepositTask(Unit unit, StorageBuildingBase building)
        {
            this.Building = building;
            Unit = unit;
        }

        public override bool IsFinished => Unit.Inventory.CurrentCapacity == 0;
        public override Vector3 TaskPosition => Building.GlobalPosition;
        public StorageBuildingBase Building { get; }
        public Unit Unit { get; }
    }
}
