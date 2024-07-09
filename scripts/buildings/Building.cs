using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

namespace SacaSimulationGame.scripts.buildings
{
    public abstract class Building
    {
        public abstract Vector2I Shape { get; }
        public abstract float MaxSlopeAngle { get; }
    }
}
