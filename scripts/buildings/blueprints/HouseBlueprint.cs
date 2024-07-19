using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using SacaSimulationGame.scripts.map;

namespace SacaSimulationGame.scripts.buildings.dataObjects
{
    public class HouseBlueprint : BuildingBlueprintBase
    {
        public override BuildingType Type => BuildingType.House;
        public override CellType[,] Shape { get; }
        public override float MaxSlopeAngle { get; }
        public override SelectionMode SelectionMode => SelectionMode.Single;
        public HouseBlueprint()
        {

            Shape = new CellType[2, 2]
            {
                { CellType.GROUND, CellType.GROUND},
                { CellType.GROUND, CellType.GROUND },
            };
            MaxSlopeAngle = 10f;
        }
    }
}
