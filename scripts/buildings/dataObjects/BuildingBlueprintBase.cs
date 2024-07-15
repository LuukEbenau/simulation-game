using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using SacaSimulationGame.scripts.map;

namespace SacaSimulationGame.scripts.buildings.dataObjects
{
    public abstract class BuildingBlueprintBase
    {
        public BuildingRotation Rotation { get; set; }
        public abstract CellType[,] Shape { get; }
        public abstract float MaxSlopeAngle { get; }
        public bool RequiresBuilding = true;
    }
}
