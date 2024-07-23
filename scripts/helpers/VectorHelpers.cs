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
        public static Vector3 ToVec3World(this Vector2 v, float height = 0)
        {
            return new Vector3(v.X, height, v.Y);
        }
        public static float DistanceTo(this Vector2I a, Vector2I b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }
        public static Vector2I Rotate(this Vector2I vector, float degrees)
        {
            // Convert to Vector2 for rotation
            Vector2 vec2 = new(vector.X, vector.Y);

            // Rotate
            Vector2 rotated = vec2.Rotated(Mathf.DegToRad(degrees));

            // Round and convert back to Vector2I
            return new Vector2I(Mathf.RoundToInt(rotated.X), Mathf.RoundToInt(rotated.Y));
        }
    }
}
