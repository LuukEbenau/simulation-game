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
        public FishingPost()
        {

            this.Shape = new CellType[8, 2]
            {
                { CellType.GROUND,CellType.GROUND},
                { CellType.WATER, CellType.WATER },
                { CellType.WATER, CellType.WATER },
                { CellType.WATER, CellType.WATER },
                { CellType.WATER, CellType.WATER },
                { CellType.WATER, CellType.WATER },
                { CellType.WATER, CellType.WATER },
                { CellType.WATER, CellType.WATER }
            };
            this.MaxSlopeAngle = 10f;
        }
    }
}
