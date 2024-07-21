using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SacaSimulationGame.scripts.buildings.storages;
using SacaSimulationGame.scripts.units;

namespace SacaSimulationGame.scripts.buildings
{
    public abstract partial class StorageBuildingBase : Building
    {
        //TODO: make this base class available for all buildings which can store resources. However, StoredResources should be a interface of different types, for stockpile, singleResourceStorage. For Lumberjack, enable pickup of Wood, for Lumbermill, dropoff wood, pickup planks, etc. This will enable behaviour for all buildings.
        public StorageBase StoredResources { get; protected set; }

        public override void _Ready()
        {
            base._Ready();
            this.StoredResources = GetNode<StorageBase>("Storage"); // new SingleResourceStorage(100, ResourceType.StockpileResources);
            this.StoredResources.StoredResourcesChanged += () => UpdateVisualBasedOnResources();
        }

        protected abstract void UpdateVisualBasedOnResources();
        public override bool IsResourceStorage => true;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceType"></param>
        /// <param name="amount"></param>
        /// <returns>Number of resources which could not be stored</returns>
        public float StoreResource(ResourceType resourceType, float amount)
        {
            return StoredResources.AddResource(resourceType, amount);
        }

        public float TakeResource(ResourceType resourceType, float amount)
        {
            return StoredResources.RemoveResource(resourceType, amount);
        }
    }
}
