using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using SacaSimulationGame.scripts.buildings.DO;

namespace SacaSimulationGame.scripts.units.tasks
{
    public class DeliverBuildingResourcesToBuildingTask
     : UnitTask
    {
        public DeliverBuildingResourcesToBuildingTask(BuildingDataObject building)
        {
            this.Building = building;
        }
        public bool _isFinished = false;
        public override bool IsFinished => _isFinished;

        public void Finish() { _isFinished = true; }

        public override Vector3 TaskPosition => Building.Instance.GlobalPosition;
        public BuildingDataObject Building { get; }



    }
}
