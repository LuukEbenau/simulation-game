using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SacaSimulationGame.scripts.buildings
{
    public partial class Lumberjack:StorageBuildingBase
    {
        public override int MaxBuilders => 2;
        public override double TotalBuildingProgressNeeded => 15;
        public override bool IsResourceStorage => false;
    
        public override BuildingType Type => BuildingType.House;
        public override void _Ready()
        {
            base._Ready();
            ResourcesRequiredForBuilding = new BuildingResources(30, 0);
            //HasResourcesToPickup = false;
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
            base._Process(delta);

            if (this.BuildingCompleted)
            {
                //temporary: passive income
                var currentResource = ResourceType.Wood;

                if (currentResource == ResourceType.None)
                {
                    var randNumber = new Random().Next(0, 2);
                    currentResource = ResourceType.Wood;
                }
                StoreResource(currentResource, (float)delta);
            }
        }

        protected override void UpdateVisualBasedOnResources()
        {
            
        }
    }
}
