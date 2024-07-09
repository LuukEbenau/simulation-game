using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

namespace SacaSimulationGame.scripts.buildings
{
    public class House: Building
    {
        public override Vector2I Shape { get; }
        public override float MaxSlopeAngle { get; }
        public House() {

            this.Shape = new Vector2I(2, 2);
            this.MaxSlopeAngle = 10f;
        }
    }
}
