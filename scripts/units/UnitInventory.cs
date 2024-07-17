using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace SacaSimulationGame.scripts.units
{

    public class UnitInventory
    {
        /// <summary>
        /// In kilos
        /// </summary>
        public float MaxCapacity => 200;
        public float CurrentCapacity => Wood + Stone;
        public float Wood { get; private set; } = 0;
        public float Stone { get; private set; } = 0;

        public float RemoveResource(ResourceType resourceType, float amount)
        {
            float depositAmount;
            switch (resourceType)
            {
                case ResourceType.Wood:
                    depositAmount = MathF.Min(amount, Wood);
                    Wood -= depositAmount;
                    break;
                case ResourceType.Stone:
                    depositAmount = MathF.Min(amount, Stone);
                    Stone -= depositAmount;
                    break;
                default:
                    throw new ArgumentException("unknown resource type");
            }

            return depositAmount;
        }
        public bool AddResource(ResourceType resourceType, float amount)
        {
            if (CurrentCapacity + amount > MaxCapacity) {
                return false;
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
            return true;
        }
    }
}
