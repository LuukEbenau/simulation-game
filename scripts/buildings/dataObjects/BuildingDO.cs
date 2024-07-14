using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using SacaSimulationGame.scripts.map;

namespace SacaSimulationGame.scripts.buildings.dataObjects
{
    public abstract class BuildingDO
    {
        public BuildingRotation Rotation { get; set; }
        public abstract CellType[,] Shape { get; }
        public abstract PackedScene Scene { get; }
        public abstract float MaxSlopeAngle { get; }
        public abstract string Name { get; }

        public abstract float TotalBuildingProgressNeeded { get; }
        public bool BuildingCompleted { get; set; } = false;
        private float _currentBuildingProgress = 0;
        public float CurrentBuildingProgress { get; set; }
    }
}
