using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using SacaSimulationGame.scripts.map;

namespace SacaSimulationGame.scripts.buildings.dataStructures.blueprints
{
    public delegate bool ElevationConstraintDelegate(float buildingBaseHeight, float cellHeight);

    public struct BuildingContraints
    {
        public CellType CellTypes { get; set; }
        /// <summary>
        /// Minimum elevation difference required from the base of the building
        /// </summary>
        public ElevationConstraintDelegate ElevationConstraint { get; set; }
        public float MaxSlope { get; set; }

    }
    public abstract class BuildingBlueprintBase
    {
        public abstract BuildingType Type { get; }
        public BuildingRotation Rotation { get; set; } = BuildingRotation.Right;
        public abstract BuildingContraints[,] CellConstraints { get; }
        //public abstract float MaxSlopeAngle { get; }
        public bool RequiresBuilding { get; set; } = true;
        public abstract SelectionMode SelectionMode { get; }
    }
}
