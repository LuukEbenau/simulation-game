using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using SacaSimulationGame.scripts.map;

namespace SacaSimulationGame.scripts.buildings.dataStructures.blueprints
{
    public class RoadBlueprint : BuildingBlueprintBase
    {
        public override BuildingType Type => BuildingType.Road;
        public override BuildingContraints[,] CellConstraints { get; }
        public override SelectionMode SelectionMode => SelectionMode.Path;
        public override Vector2I EntranceCell { get; } = new Vector2I(0, 0);
        public RoadBlueprint()
        {
            CalculateCellHeightDelegate ch = (float cellHeight, float baseHeight) => cellHeight;
            CellConstraints = new BuildingContraints[1, 1]
            {
                { new BuildingContraints{ MaxSlope = 20f, CellTypes = CellType.GROUND, CalculateHeight = ch}}
            };
        }
    }
}
