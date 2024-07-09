using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using SacaSimulationGame.scripts.map;

namespace SacaSimulationGame.scripts.buildings
{
    internal class FishingPost: Building
    {
        public override CellType[,] Shape { get; }
        public override float MaxSlopeAngle { get; }
        public override string Name => "FishingPost";
        private static PackedScene _scene;
        public override PackedScene Scene => _scene ??= GD.Load("res://assets/buildings/house.blend") as PackedScene;
        public FishingPost()
        {

            this.Shape = new CellType[5, 2]
            {
                { CellType.GROUND,CellType.GROUND},
                { CellType.WATER | CellType.GROUND, CellType.WATER | CellType.GROUND },
                { CellType.WATER, CellType.WATER },
                { CellType.WATER, CellType.WATER },
                { CellType.WATER, CellType.WATER },
            };
            this.MaxSlopeAngle = 15f;
        }
    }
}
