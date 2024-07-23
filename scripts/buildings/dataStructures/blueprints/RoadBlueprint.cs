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
        public override CellType[,] Shape { get; }
        public override float MaxSlopeAngle { get; }
        public override SelectionMode SelectionMode => SelectionMode.Path;
        public RoadBlueprint()
        {
            Shape = new CellType[1, 1]
            {
                { CellType.GROUND}
            };
            MaxSlopeAngle = 20f;
        }


    }
}
