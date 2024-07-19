using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using SacaSimulationGame.scripts.map;

namespace SacaSimulationGame.scripts.buildings.dataObjects
{
    public class StockpileBlueprint : BuildingBlueprintBase
    {
        public override BuildingType Type => BuildingType.Stockpile;
        public override CellType[,] Shape { get; }
        public override float MaxSlopeAngle { get; }
        public override SelectionMode SelectionMode => SelectionMode.Single;
        public StockpileBlueprint()
        {

            Shape = new CellType[1, 1]
            {
                { CellType.GROUND}
            };
            MaxSlopeAngle = 15f;
        }
    }
}
