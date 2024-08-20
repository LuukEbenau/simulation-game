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
    /// A storage in which each resource has their own capacity limit, rather than total capacity
    /// </summary>
    public partial class ResourceCappedStorage : StorageBase
    {
        //[Export] public Godot.Collections.Dictionary<string, float> StorageCapacity { get; set; }

        [Export] public float WoodCapacity { get; set; }
        [Export] public float StoneCapacity { get; set; }
        [Export] public float FishCapacity { get; set; }

        [Export] public ResourceType InputResources { get; set; }
        [Export] public ResourceType OutputResources { get; set; }
        public override ResourceType InputResourceTypes => InputResources;

        public override ResourceType OutputResourceTypes => OutputResources;

        public override float AddResource(ResourceType resourceType, float amount)
        {
            //if (InputResourceTypes.HasFlag(resourceType))
            //{
            return base.AddResource(resourceType, amount);
            //}
            //else
            //{
            //    throw new Exception($"resource of type {resourceType} can not be stored");
            //}
        }
        public override float RemoveResource(ResourceType resourceType, float amount)
        {
            if (OutputResourceTypes.HasFlag(resourceType))
            {
                return base.RemoveResource(resourceType, amount);
            }
            else
            {
                throw new Exception($"resource of type {resourceType} can not be removed");
            }
        }

        private float GetResourceCapacity(ResourceType resourceType)
        {
            var amount = 0f;
            if (resourceType.HasFlag(ResourceType.Wood)) amount += WoodCapacity;
            if (resourceType.HasFlag(ResourceType.Stone)) amount += StoneCapacity;
            if (resourceType.HasFlag(ResourceType.Fish)) amount += FishCapacity;

            return amount;
        }


        public override float GetStorageCapacityLeft(ResourceType type)
        {
            //ValidateResourceTypeIsPure(type);

            var currentResources = GetResourcesOfType(type);
            var capacity = GetResourceCapacity(type);

            return capacity - currentResources;

        }
    }
}
