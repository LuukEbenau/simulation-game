using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SacaSimulationGame.scripts.buildings
{
    public partial class Lumberjack:Building
    {
        public override int MaxBuilders => 2;
        public override double TotalBuildingProgressNeeded => 15;
        public override bool IsResourceStorage => false;
        
        //public override bool ResourceDropoffEnabled
        public override BuildingType Type => BuildingType.House;
        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            base._Ready();
            ResourcesRequiredForBuilding = new BuildingResources(30, 10);
            //HasResourcesToPickup = false;
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
            base._Process(delta);
        }
    }
}
