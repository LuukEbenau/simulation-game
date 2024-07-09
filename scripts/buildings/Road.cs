﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

namespace SacaSimulationGame.scripts.buildings
{
    public class Road : Building
    {
        public override Vector2I Shape { get; }
        public override float MaxSlopeAngle { get; }
        public Road()
        {
            this.Shape = new Vector2I(1, 1);
            this.MaxSlopeAngle = 20f;
        }
    }
}
