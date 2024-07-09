using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

namespace SacaSimulationGame.scripts.buildings
{
    internal class FishingPost: Building
    {
        public override Vector2I Shape { get; }
        public override float MaxSlopeAngle { get; }
        public FishingPost()
        {

            this.Shape = new Vector2I(2, 8);
            this.MaxSlopeAngle = 10f;
        }
    }
}
