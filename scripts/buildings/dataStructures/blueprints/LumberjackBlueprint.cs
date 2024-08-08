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
        public LumberjackBlueprint()
        {
            var c = new BuildingContraints { CellTypes = CellType.GROUND, MaxSlope = 10f };
            CellConstraints = new BuildingContraints[2, 2]
            {
                { c, c},
                { c, c},
            };
        }
    }
}
