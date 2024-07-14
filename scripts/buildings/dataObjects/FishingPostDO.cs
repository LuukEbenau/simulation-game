using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using SacaSimulationGame.scripts.map;

namespace SacaSimulationGame.scripts.buildings.dataObjects
{
    internal class FishingPostDO : BuildingDO
    {
        public override CellType[,] Shape { get; }
        public override float MaxSlopeAngle { get; }
        public override string Name => "FishingPost";
        private static PackedScene _scene;
        public override PackedScene Scene => _scene ??= GD.Load("res://assets/buildings/house.blend") as PackedScene;
        public FishingPostDO()
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

        public override float TotalBuildingProgressNeeded => 30;
    }
}
