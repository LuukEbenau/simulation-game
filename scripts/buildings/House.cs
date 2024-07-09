using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

namespace SacaSimulationGame.scripts.buildings
{
    public class House
    {
        public Vector2I Shape { get; }
        public float MaxSlopeAngle { get; }
        public House() {

            this.Shape = new Vector2I(3, 3);
            this.MaxSlopeAngle = 7f;
        }
    }
}
