using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SacaSimulationGame.scripts.buildings.storages;
using SacaSimulationGame.scripts.units;

namespace SacaSimulationGame.scripts.buildings
{
    public abstract partial class StorageBuildingBase : BuildingBase
    {
        public StorageBase StoredResources { get; protected set; }

        public override void _Ready()
        {
            base._Ready();
            this.StoredResources = GetNode<StorageBase>("Storage");
            this.StoredResources.StoredResourcesChanged += () => UpdateVisualBasedOnResources();
        }

        protected abstract void UpdateVisualBasedOnResources();
        public override bool IsResourceStorage => true;
    }
}
