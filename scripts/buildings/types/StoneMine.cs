using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SacaSimulationGame.scripts.naturalResources;
using SacaSimulationGame.scripts.units.dataObjects;
using SacaSimulationGame.scripts.units.professions.misc;

namespace SacaSimulationGame.scripts.buildings
{
    public partial class StoneMine : StorageBuildingBase
    {
        public override int MaxBuilders => 2;
        public override float TotalBuildingProgressNeeded => 20;
        public override bool IsResourceStorage => false;

        public override BuildingType Type => BuildingType.Lumberjack;
        public override void _Ready()
        {
            base._Ready();
            OnBuildingCompleted += House_OnBuildingCompleted;
        }


        private void House_OnBuildingCompleted()
        {
            GameManager.UnitManager.SpawnUnit(GlobalPosition, new UnitDataObject(UnitGender.MALE, ProfessionType.Lumberjack));
        }


        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
            base._Process(delta);

            if (this.BuildingCompleted)
            {
                //temporary: passive income
                var currentResource = ResourceType.Stone;

                StoredResources.AddResource(currentResource, (float)delta);
            }
        }

        protected override void UpdateVisualBasedOnResources()
        {

        }
    }
}
