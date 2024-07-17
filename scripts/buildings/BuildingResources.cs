using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading.Tasks;
using SacaSimulationGame.scripts.units;

namespace SacaSimulationGame.scripts.buildings
{
    public class BuildingResources(float wood, float stone)
    {
        public float PercentageResourcesAquired => (CurrentWood + CurrentStone) / (Wood + Stone);
        public bool RequiresResources => PercentageResourcesAquired < 1;
        public float Wood { get; } = wood;
        public float CurrentWood { get; private set; }
        public float Stone { get; } = stone;
        public float CurrentStone { get; private set; }

        public float RequiresOfResource(ResourceType resourceType)
        {
            if (resourceType == ResourceType.Wood)
            {
                return  Wood - CurrentWood;
            }
            else if (resourceType == ResourceType.Stone)
            {
                return Stone - CurrentStone;
            }
            else throw new Exception($"Resource type {resourceType} not implemented");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceType"></param>
        /// <param name="amount"></param>
        /// <returns>Amount of leftover of the resource</returns>
        public float Deposit(ResourceType resourceType, float amount)
        {
            if (resourceType == ResourceType.Wood)
            {
                var spaceLeft = Wood - CurrentWood;
                if (amount <= spaceLeft)
                {
                    CurrentWood += amount;
                }
                else
                {
                    var leftover = amount - spaceLeft;
                    CurrentWood += spaceLeft;
                    return leftover;
                }
            }
            else if (resourceType == ResourceType.Stone)
            {
                var spaceLeft = Stone - CurrentStone;
                if (amount <= spaceLeft)
                {
                    CurrentStone += amount;
                }
                else
                {
                    var leftover = amount - spaceLeft;
                    CurrentStone += spaceLeft;

                    return leftover;
                }
            }

            return 0;
        }
    }
}
