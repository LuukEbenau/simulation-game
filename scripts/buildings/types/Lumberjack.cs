using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SacaSimulationGame.scripts.naturalResources;
using SacaSimulationGame.scripts.units.dataObjects;
using SacaSimulationGame.scripts.units.professions.misc;
using SacaSimulationGame.scripts.units.tasks;

namespace SacaSimulationGame.scripts.buildings
{
    public partial class Lumberjack : StorageBuildingBase
    {
        public override int MaxBuilders => 2;
        public override float TotalBuildingProgressNeeded => 5;
        public override StorageStrategyEnum StorageStrategy => StorageStrategyEnum.EmptyAllResources;
        public override BuildingType Type => BuildingType.Lumberjack;
        public override void _Ready()
        {
            base._Ready();
            OnBuildingCompleted += House_OnBuildingCompleted;

            StoredResources.StoredResourcesChanged += StoredResources_StoredResourcesChanged;
        }

        private void StoredResources_StoredResourcesChanged()
        {
            if (StoredResources.CurrentCapacity > 0)
            {
                var currentTask = GameManager.TaskManager.GetTasks().Where(t => t is PickupResourcesTask pt && pt.Building == this).FirstOrDefault();
                if (currentTask == null) {
                    var task = new PickupResourcesTask(this);
                    GameManager.TaskManager.EnqueueTask(task);
                }
            }
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
                var currentResource = ResourceType.Wood;

                StoredResources.AddResource(currentResource, (float)delta);
            }
        }

        protected override void UpdateVisualBasedOnResources()
        {
            
        }
    }
}
