using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using SacaSimulationGame.scripts.map;

namespace SacaSimulationGame.scripts.buildings.dataStructures.blueprints
{

    public abstract class BuildingBlueprintBase
    {
        public abstract BuildingType Type { get; }
        public BuildingRotation Rotation { get; set; } = BuildingRotation.Right;
        public abstract BuildingContraints[,] CellConstraints { get; }
        //public abstract float MaxSlopeAngle { get; }
        public bool RequiresBuilding { get; set; } = true;
        public abstract SelectionMode SelectionMode { get; }
        public BuildingContraints BaseCellConstraintOverride { get; set; }
        public BuildingContraints DestinationCellConstraintOverride { get; set; }
        public abstract Vector2I EntranceCell { get; }
        /// <summary>
        /// Supplies a convenient way to get constraints which automatically merges overrides
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="isBase"></param>
        /// <param name="isDestination"></param>
        /// <returns></returns>
        public BuildingContraints GetConstraints(int x, int y, bool isBase = false, bool isDestination = false)
        {
            var buildingContraints = CellConstraints[x, y];

            // We apply the base and destination overrides of applicable
            if (isDestination && DestinationCellConstraintOverride != default)
            {
                var or = DestinationCellConstraintOverride;
                buildingContraints = new BuildingContraints
                {
                    CalculateHeight = or.CalculateHeight ?? buildingContraints.CalculateHeight,
                    CellTypes = or.CellTypes != default ? or.CellTypes : buildingContraints.CellTypes,
                    ElevationConstraint = or.ElevationConstraint ?? buildingContraints.ElevationConstraint,
                    MaxSlope = or.MaxSlope != default ? or.MaxSlope : buildingContraints.MaxSlope
                };
            }
            if (isBase && BaseCellConstraintOverride != default)
            {
                var or = BaseCellConstraintOverride;
                buildingContraints = new BuildingContraints
                {
                    CalculateHeight = or.CalculateHeight ?? buildingContraints.CalculateHeight,
                    CellTypes = or.CellTypes != default ? or.CellTypes : buildingContraints.CellTypes,
                    ElevationConstraint = or.ElevationConstraint ?? buildingContraints.ElevationConstraint,
                    MaxSlope = or.MaxSlope != default ? or.MaxSlope : buildingContraints.MaxSlope
                };
            }

            return buildingContraints;
        }
    }
}
