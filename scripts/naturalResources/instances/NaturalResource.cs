using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using SacaSimulationGame.scripts.buildings.storages;
using SacaSimulationGame.scripts.managers;
using SacaSimulationGame.scripts.naturalResources.instances;

namespace SacaSimulationGame.scripts.naturalResources
{
    public partial class NaturalResource : Node3D, INaturalResource
    {
        protected StorageBase ResourceStorage { get; set; }
        protected Node3D VisualWrap { get; set; }

        public Vector2I Cell { get; set; }
        public NaturalResourceManager NaturalResourceManager { get; set; }
        public override void _Ready()
        {
            base._Ready();
            ResourceStorage = GetNode<StorageBase>("ResourceStorage");
            VisualWrap = GetNode<Node3D>("VisualWrap");
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="amount"></param>
        /// <returns>Actual amount of resource taken</returns>
        public (float amount, ResourceType type) CollectResource(float amount)
        {
            //NOTE: actual behaviour can be more complex, such as yielding occasional gold from iron, etc.
            var type = ResourceStorage.TypesOfResourcesStored;
            if(type == 0)
            {
                return default;
            }
            return (ResourceStorage.RemoveResource(type, amount), type);
        }

        public float GetNrOfResourcesLeft()
        {
            return ResourceStorage.CurrentCapacity;
        }

    }
}
