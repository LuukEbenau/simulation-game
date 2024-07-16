using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using SacaSimulationGame.scripts.map;

namespace SacaSimulationGame.scripts.buildings.dataObjects
{
    internal class FishingPostBlueprint : BuildingBlueprintBase
    {
        public override BuildingType Type => BuildingType.FishingPost;
        public override SelectionMode SelectionMode => SelectionMode.Single;
        public override CellType[,] Shape { get; }
        public override float MaxSlopeAngle { get; }

        public FishingPostBlueprint()
        {

            Shape = new CellType[5, 2]
            {
                { CellType.GROUND,CellType.GROUND},
                { CellType.WATER | CellType.GROUND, CellType.WATER | CellType.GROUND },
                { CellType.WATER, CellType.WATER },
                { CellType.WATER, CellType.WATER },
                { CellType.WATER, CellType.WATER },
            };
            MaxSlopeAngle = 15f;
        }
    }
}
