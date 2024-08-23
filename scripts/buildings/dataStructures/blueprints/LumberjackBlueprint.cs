using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using SacaSimulationGame.scripts.map;

namespace SacaSimulationGame.scripts.buildings.dataStructures.blueprints
{
    public class LumberjackBlueprint : BuildingBlueprintBase
    {
        public override BuildingType Type => BuildingType.Stockpile;
        public override BuildingContraints[,] CellConstraints { get; }
        public override SelectionMode SelectionMode => SelectionMode.Single;
        public override Vector2I EntranceCell { get; } = new Vector2I(0, 0);
        public LumberjackBlueprint()
        {
            CalculateCellHeightDelegate ch = (float cellHeight, float baseHeight) => cellHeight;
            var c = new BuildingContraints { CellTypes = CellType.GROUND, MaxSlope = 10f , CalculateHeight = ch };
            CellConstraints = new BuildingContraints[2, 2]
            {
                { c, c},
                { c, c},
            };
        }
    }
}
