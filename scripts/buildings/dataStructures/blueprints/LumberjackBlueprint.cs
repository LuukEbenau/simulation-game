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
        public override CellType[,] Shape { get; }
        public override float MaxSlopeAngle { get; }
        public override SelectionMode SelectionMode => SelectionMode.Single;
        public LumberjackBlueprint()
        {

            Shape = new CellType[2, 2]
            {
                { CellType.GROUND, CellType.GROUND},
                { CellType.GROUND, CellType.GROUND}
            };
            MaxSlopeAngle = 15f;
        }
    }
}
