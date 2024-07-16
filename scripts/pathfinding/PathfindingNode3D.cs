using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

namespace SacaSimulationGame.scripts.pathfinding
{
    public struct PathfindingNode3D(Vector3 position, float speedMultiplier)
    {
        public Vector3 Position { get; } = position;
        public float SpeedMultiplier { get; } = speedMultiplier;

        public override readonly string ToString()
        {
            return Position.ToString();
        }
    }
}
