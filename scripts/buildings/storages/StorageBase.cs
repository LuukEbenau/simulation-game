﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

namespace SacaSimulationGame.scripts.buildings.storages
{
    public abstract partial class StorageBase : Node3D
    {
        [Export] public float MaxCapacity { get; set; }

        [Signal]
        public delegate void StoredResourcesChangedEventHandler();

        //public float StorageSpaceLeft => MaxCapacity - CurrentCapacity;
        //public abstract float CurrentCapacity => Wood + Stone;

        public float CurrentCapacity => Wood + Stone;

        /// <summary>
        /// Allowed resources to be deposited
        /// </summary>
        public abstract ResourceType InputResourceTypes { get; }
        /// <summary>
        /// Allowed Resources to be removed
        /// </summary>
        public abstract ResourceType OutputResourceTypes { get; }
        public ResourceType TypesOfResourcesStored { get; private set; } = 0;

        public abstract float GetStorageSpaceLeft(ResourceType type);

        public float Wood { get; private set; } = 0;
        public float Stone { get; private set; } = 0;


        public float GetResourcesOfType(ResourceType type)
        {
            if (type == ResourceType.Wood) return Wood;
            if (type == ResourceType.Stone) return Stone;

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
        public virtual float RemoveResource(ResourceType resourceType, float amount)
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
            if (GetResourcesOfType(resourceType) == 0) TypesOfResourcesStored ^= resourceType;

            if (removedAmount > 0)
            {
                var result = EmitSignal(SignalName.StoredResourcesChanged);
                if (result == Error.Ok)
                {
                    GD.Print("signal succes");
                }
                else
                {
                    GD.Print("signal failed");
                }
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
            if (GetStorageSpaceLeft(resourceType) <= amount)
            {
                GD.Print($"could not store {amount} resources of type {resourceType}");
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

            
            var result = EmitSignal(SignalName.StoredResourcesChanged);
            if (result == Error.Ok)
            {
                GD.Print("signal succes");
            }
            else
            {
                GD.Print("signal failed");
            }
            return 0;
        }
    }
}
