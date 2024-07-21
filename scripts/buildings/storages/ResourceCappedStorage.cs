using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using Godot;

namespace SacaSimulationGame.scripts.buildings.storages
{
    /// <summary>
    /// A storage in which each resource has their own capacity limit, rather than total capacity
    /// </summary>
    public partial class ResourceCappedStorage : StorageBase
    {
        [Export] public Godot.Collections.Dictionary<string, float> StorageCapacity { get; set; }

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

        public override float GetStorageSpaceLeft(ResourceType type)
        {
            ValidateResourceTypeIsPure(type);

            var currentResources = GetResourcesOfType(type);
            if(StorageCapacity.TryGetValue(type.ToString(), out var storageSpace))
            {
                return storageSpace - currentResources;
            }
            else
            {
                throw new Exception($"No storage capacity defined for resource {type} on building {nameof(ResourceCappedStorage)}");
            }
        }
    }
}
