using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace SacaSimulationGame.scripts.units
{

    public class UnitInventory(float maxCapacity = 200)
    {
        /// <summary>
        /// In kilos
        /// </summary>
        public float MaxCapacity => maxCapacity;
        public float StorageSpaceLeft => MaxCapacity - CurrentCapacity;
        public float CurrentCapacity => Wood + Stone;
        public float Wood { get; private set; } = 0;
        public float Stone { get; private set; } = 0;

        public ResourceType TypesOfResourcesStored { get; private set; } = 0;

        public float GetResourcesOfType(ResourceType type)
        {
            if (type == ResourceType.Wood) return Wood;
            if(type== ResourceType.Stone) return Stone;

            throw new Exception("Resource type not implemented yet");
            //return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceType"></param>
        /// <param name="amount"></param>
        /// <returns>amount of the resource taken</returns>
        /// <exception cref="ArgumentException"></exception>
        public float RemoveResource(ResourceType resourceType, float amount)
        {
            float removedAmount;
            switch (resourceType)
            {
                case ResourceType.Wood:
                    removedAmount = MathF.Min(amount, Wood);
                    Wood -= removedAmount;
                    break;
                case ResourceType.Stone:
                    removedAmount = MathF.Min(amount, Stone);
                    Stone -= removedAmount;
                    break;
                default:
                    throw new ArgumentException("unknown resource type");
            }

            // remove flag if the resource is empty
            if (GetResourcesOfType(resourceType) == 0) TypesOfResourcesStored &= ~resourceType;

            return removedAmount;
        }
        public bool AddResource(ResourceType resourceType, float amount)
        {
            if (CurrentCapacity + amount > MaxCapacity) {
                return false; //TODO: can't we just store it and return back the resources which couldnt be picked up?
            }
            switch (resourceType)
            {
                case ResourceType.Wood:
                    Wood += amount;
                    break;
                case ResourceType.Stone:
                    Stone += amount;
                    break;
            }
            TypesOfResourcesStored |= resourceType; // add flag

            return true;
        }
    }
}
