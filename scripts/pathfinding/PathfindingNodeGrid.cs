using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

namespace SacaSimulationGame.scripts.pathfinding
{
    public readonly struct PathfindingNodeGrid(Vector2I cell, float speedMultiplier)
    {
        public Vector2I Cell { get; } = cell;
        public float SpeedMultiplier { get; } = speedMultiplier;
        public override readonly bool Equals([NotNullWhen(true)] object obj)
        {
            if (obj is PathfindingNodeGrid n)
            {
                return Cell.Equals(n.Cell);
            }
            return false;
        }
        public override readonly int GetHashCode()
        {
            return Cell.GetHashCode();
        }
        public override readonly string ToString()
        {
            return Cell.ToString();
        }
    }
}
