using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using SacaSimulationGame.scripts.buildings;
using SacaSimulationGame.scripts.buildings.DO;

namespace SacaSimulationGame.scripts.units.tasks
{
    public class PickupResourcesTask : UnitTask
    {
        public PickupResourcesTask(StorageBuildingBase building)
        {
            this.Building = building;
        }
        //public bool _isFinished = false;
        //public void Finish()
        //{
        //    _isFinished = true;
        //}
        public override bool IsFinished => Building.StoredResources.CurrentCapacity == 0;
        public override Vector3 TaskPosition => Building.GlobalPosition;
        public StorageBuildingBase Building { get; }



    }
}
