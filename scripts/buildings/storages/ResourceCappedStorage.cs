using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SacaSimulationGame.scripts.buildings.storages
{
    public partial class ResourceCappedStorage : StorageBase
    {
        public override ResourceType InputResourceTypes => throw new NotImplementedException();

        public override ResourceType OutputResourceTypes => throw new NotImplementedException();

        public override float GetStorageSpaceLeft(ResourceType type)
        {
            throw new NotImplementedException();
        }
    }
}
