using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using SacaSimulationGame.scripts.map;

namespace SacaSimulationGame.scripts.buildings
{
    public class Road : Building
    {
        public override CellType[,] Shape { get; }
        public override float MaxSlopeAngle { get; }
        public Road()
        {
            this.Shape = new CellType[1, 1]
            {
                { CellType.GROUND}
            };
            this.MaxSlopeAngle = 20f;
        }
    }
}
