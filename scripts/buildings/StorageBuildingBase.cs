using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SacaSimulationGame.scripts.units;

namespace SacaSimulationGame.scripts.buildings
{
    public abstract partial class StorageBuildingBase : Building
    {
        //TODO: make this base class available for all buildings which can store resources. However, StoredResources should be a interface of different types, for stockpile, singleResourceStorage. For Lumberjack, enable pickup of Wood, for Lumbermill, dropoff wood, pickup planks, etc. This will enable behaviour for all buildings.
        protected UnitInventory StoredResources { get; set; } = new UnitInventory(maxCapacity: 100);
        public ResourceType CurrentResourceStored { get; protected set; } = ResourceType.None;

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
            HasResourcesToPickup = true;

            if (CurrentResourceStored == ResourceType.None || resourceType == CurrentResourceStored)
            {
                CurrentResourceStored = resourceType;
                if (amount < StoredResources.StorageSpaceLeft)
                {
                    StoredResources.AddResource(resourceType, amount);

                    UpdateVisualBasedOnResources();
                    return 0;
                }
                else
                {
                    StoredResources.AddResource(resourceType, StoredResources.StorageSpaceLeft);
                    var leftover = amount - StoredResources.StorageSpaceLeft;

                    UpdateVisualBasedOnResources();
                    return leftover;
                }


            }

            return amount;
        }

        public float TakeResource(ResourceType resourceType, float amount)
        {
            float nrOfResourcesTaken = 0;

            if (resourceType == CurrentResourceStored)
            {
                nrOfResourcesTaken = StoredResources.RemoveResource(resourceType, amount);

                if (StoredResources.CurrentCapacity == 0)
                {
                    HasResourcesToPickup = false;
                    CurrentResourceStored = ResourceType.None;
                }

                UpdateVisualBasedOnResources();
            }

            return nrOfResourcesTaken;
        }
    }
}
