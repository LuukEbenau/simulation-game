using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

namespace SacaSimulationGame.scripts.pathfinding
{
    public struct PathfindingNode3D
    {
        public PathfindingNode3D(Vector3 position, float speedMultiplier)
        {
            this.Position = position;
            this.SpeedMultiplier = speedMultiplier;
        }
        public Vector3 Position { get; }
        public float SpeedMultiplier { get; } 

        public override readonly string ToString()
        {
            return Position.ToString();
        }
    }
}
