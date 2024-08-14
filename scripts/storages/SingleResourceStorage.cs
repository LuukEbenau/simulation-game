﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using SacaSimulationGame.scripts.naturalResources;

namespace SacaSimulationGame.scripts.buildings.storages
{
    /// <summary>
    /// Resource storage which can only hold 1 single resource at the same time
    /// </summary>
    public partial class SingleResourceStorage : StorageBase
    {
        [Export] public float MaxCapacity { get; set; }
        [Export] public ResourceType StorableResources { get; set; } = ResourceType.AllResources;
        public bool HasResourcesToPickup { get; set; } = false;
        public ResourceType CurrentResourceStored { get; protected set; } = 0;
        public override ResourceType InputResourceTypes { get; }

        public override ResourceType OutputResourceTypes { get; }

        

        public override float AddResource(ResourceType resourceType, float amount)
        {
            HasResourcesToPickup = true;

            if (CurrentResourceStored == 0 || resourceType == CurrentResourceStored)
            {
                CurrentResourceStored = resourceType;
                if (amount < GetStorageCapacityLeft(resourceType))
                {
                    base.AddResource(resourceType, amount);

                    return 0;
                }
                else
                {
                    base.AddResource(resourceType, GetStorageCapacityLeft(resourceType));
                    var leftover = amount - GetStorageCapacityLeft(resourceType);

                    return leftover;
                }
            }

            return amount;
        }

        public override float GetStorageCapacityLeft(ResourceType type)
        {
            return MaxCapacity - CurrentCapacity;
        }

        public override float RemoveResource(ResourceType resourceType, float amount)
        {
            float nrOfResourcesTaken = 0;

            if (CurrentResourceStored.HasFlag(resourceType))
            {

                nrOfResourcesTaken = base.RemoveResource(resourceType, amount);

                if (GetResourcesOfType(resourceType) == 0)
                {
                    HasResourcesToPickup = false;
                    CurrentResourceStored &= ~resourceType;
                }
            }

            return nrOfResourcesTaken;
        }
    }
}
