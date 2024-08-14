﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using SacaSimulationGame.scripts.naturalResources;

namespace SacaSimulationGame.scripts.buildings.storages
{
    public partial class GeneralStorage : StorageBase
    {
        [Export] public float MaxCapacity { get; set; }
        [Export] public ResourceType StorableResources { get; set; } = ResourceType.AllResources;
    

        public override ResourceType InputResourceTypes => StorableResources;

        public override ResourceType OutputResourceTypes => StorableResources;

        //public ResourceType StorableResources { get; } = storableResources;

        public override float GetStorageCapacityLeft(ResourceType type)
        {
            return MaxCapacity - CurrentCapacity;
        }
    }
}
