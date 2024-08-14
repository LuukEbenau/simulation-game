﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using SacaSimulationGame.scripts.naturalResources;


namespace SacaSimulationGame.scripts.buildings.storages
{
    public abstract partial class StorageBase : Node3D
    {
        [Signal]
        public delegate void StoredResourcesChangedEventHandler();

        public float CurrentCapacity => Wood + Stone + Fish;

        /// <summary>
        /// Allowed resources to be deposited
        /// </summary>
        public abstract ResourceType InputResourceTypes { get; }
        /// <summary>
        /// Allowed Resources to be removed
        /// </summary>
        public abstract ResourceType OutputResourceTypes { get; }
        public ResourceType TypesOfResourcesStored { get; private set; } = 0;

        public abstract float GetStorageCapacityLeft(ResourceType type);

        public virtual float Wood { get; set; } = 0;
        public virtual float Stone { get; set; } = 0;
        public virtual float Fish { get; set; } = 0;

        protected bool ValidateResourceTypeIsPure(ResourceType resourceType)
        {
            if ((resourceType & (resourceType - 1)) != 0)
            {
                throw new Exception($"More than 1 resource selected {resourceType}");
            }
            return true;
        }

        public float GetResourcesOfType(ResourceType resourceType)
        {
            //ValidateResourceTypeIsPure(resourceType);
            float resources = 0;
            var resourceFound = false;

            if (resourceType.HasFlag(ResourceType.Wood))
            {
                resources += Wood;
                resourceFound = true;
            }

            if (resourceType.HasFlag(ResourceType.Stone))
            {
                resources += Stone;
                resourceFound = true;
            }

            if (resourceType.HasFlag(ResourceType.Fish))
            {
                resources += Fish;
                resourceFound = true;
            }

            if(!resourceFound) throw new Exception($"Resource type not implemented yet  {resourceType}");

            return resources;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceType"></param>
        /// <param name="amount"></param>
        /// <returns>amount of the resource taken</returns>
        /// <exception cref="ArgumentException"></exception>
        public virtual float RemoveResource(ResourceType resourceType, float amount)
        {
            ValidateResourceTypeIsPure(resourceType);

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

            if (removedAmount > 0)
            {
                EmitSignal(SignalName.StoredResourcesChanged);

            }

            return removedAmount;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceType"></param>
        /// <param name="amount"></param>
        /// <returns>Amount of resources which could not be stored</returns>
        public virtual float AddResource(ResourceType resourceType, float amount)
        {
            ValidateResourceTypeIsPure(resourceType);
            var storageSpaceLeft = GetStorageCapacityLeft(resourceType);
            if (storageSpaceLeft < amount)
            {
                //GD.Print($"could not store {amount} resources of type {resourceType}");
                return amount; //TODO: can't we just store it and return back the resources which couldnt be picked up?
            }
            switch (resourceType)
            {
                case ResourceType.Wood:
                    Wood += amount;
                    break;
                case ResourceType.Stone:
                    Stone += amount;
                    break;
                default: throw new ArgumentException($"Unknown resource type: {resourceType}");
            }
            TypesOfResourcesStored |= resourceType; // add flag

            
            EmitSignal(SignalName.StoredResourcesChanged);
            return 0;
        }
    }
}
