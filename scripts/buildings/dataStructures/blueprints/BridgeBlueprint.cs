using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SacaSimulationGame.scripts.map;

namespace SacaSimulationGame.scripts.buildings.dataStructures.blueprints
{
    public class BridgeBlueprint : BuildingBlueprintBase
    {
        public override BuildingType Type => BuildingType.Bridge;
        public override BuildingContraints[,] CellConstraints { get; }
        public override SelectionMode SelectionMode => SelectionMode.Line;

        public BridgeBlueprint()
        {
            CellConstraints = new BuildingContraints[1, 1]
            {
                { new BuildingContraints { 
                    MaxSlope = 255f, 
                    CellTypes = CellType.WATER,
                    CalculateHeight = (float cellHeight, float baseHeight) => baseHeight
                } }
            };

            static bool destinationElevationConstraint(float buildingBaseHeight, float cellHeight)
            {
                return true;
                //float elevationHeight = 5f;
                //return (buildingBaseHeight - cellHeight) >= elevationHeight;
            }

            BaseCellConstraintOverride = new BuildingContraints
            {
                CellTypes = CellType.GROUND
            };
            DestinationCellConstraintOverride = new BuildingContraints
            {
                CellTypes = CellType.GROUND,
                ElevationConstraint = destinationElevationConstraint,
                CalculateHeight = (float cellHeight, float baseHeight) => baseHeight //TODO, instead, it needs to have an gradient from the baseheight to cellheight
            };


        }
    }
}
