using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using SacaSimulationGame.scripts.map;

namespace SacaSimulationGame.scripts.buildings
{
    public class House: Building
    {
        public override CellType[,] Shape { get; }
        public override float MaxSlopeAngle { get; }
        public House() {
            this.Shape = new CellType[2, 2]
            {
                { CellType.GROUND, CellType.GROUND},
                { CellType.GROUND, CellType.GROUND },
            };
            this.MaxSlopeAngle = 10f;
        }
    }
}
