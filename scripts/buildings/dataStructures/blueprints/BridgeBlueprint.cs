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
        public override SelectionMode SelectionMode => SelectionMode.Path;
        public BridgeBlueprint()
        {
            CellConstraints = new BuildingContraints[1, 1]
            {
                { new BuildingContraints{ MaxSlope = 255f, CellTypes = CellType.WATER}} 
                //TODO: it should be that it starts as ground and ends as ground, but is water in between
            };
        }
    }
}
