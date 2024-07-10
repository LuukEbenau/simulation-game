using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

namespace SacaSimulationGame.scripts
{
    public static class VectorHelpers
    {
        public static Vector2I ToVec2World(this Vector3I v)
        {
            return new Vector2I(v.X, v.Z);
        }
        public static Vector3 ToVec3World(this Vector2 v, int height = 0)
        {
            return new Vector3(v.X, height, v.Y);
        }
    }
}
