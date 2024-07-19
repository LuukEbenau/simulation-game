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
        public UnitInventory StoredResources { get; set; } = new UnitInventory(maxCapacity: 250);
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
            if (resourceType == ResourceType.None || resourceType == CurrentResourceStored)
            {
                CurrentResourceStored = resourceType;
                if (amount < StoredResources.CurrentCapacity)
                {
                    StoredResources.AddResource(resourceType, amount);

                    UpdateVisualBasedOnResources();
                    return 0;
                }
                else
                {
                    StoredResources.AddResource(resourceType, StoredResources.CurrentCapacity);
                    var leftover = StoredResources.CurrentCapacity - amount;

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
                    CurrentResourceStored = ResourceType.None;
                }

                UpdateVisualBasedOnResources();
            }

            return nrOfResourcesTaken;
        }
    }
}
