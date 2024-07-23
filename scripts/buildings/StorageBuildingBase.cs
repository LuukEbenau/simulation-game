using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SacaSimulationGame.scripts.buildings.storages;
using SacaSimulationGame.scripts.units;

namespace SacaSimulationGame.scripts.buildings
{
    public abstract partial class StorageBuildingBase : Building
    {
        //TODO: make this base class available for all buildings which can store resources. However, StoredResources should be a interface of different types, for stockpile, singleResourceStorage. For Lumberjack, enable pickup of Wood, for Lumbermill, dropoff wood, pickup planks, etc. This will enable behaviour for all buildings.
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
