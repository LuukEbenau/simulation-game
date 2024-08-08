using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using SacaSimulationGame.scripts.map;

namespace SacaSimulationGame.scripts.buildings.dataStructures.blueprints
{
    public class StockpileBlueprint : BuildingBlueprintBase
    {
        public override BuildingType Type => BuildingType.Stockpile;
        public override BuildingContraints[,] CellConstraints { get; }
        public override SelectionMode SelectionMode => SelectionMode.Single;
        public StockpileBlueprint()
        {

            CellConstraints = new BuildingContraints[1, 1]
            { 
                { new BuildingContraints{MaxSlope = 15f, CellTypes= CellType.GROUND} }
            };
        }
    }
}
