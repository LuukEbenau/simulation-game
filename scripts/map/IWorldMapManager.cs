using System.Collections.Generic;
using Godot;
using SacaSimulationGame.scripts.pathfinding;

namespace SacaSimulationGame.scripts.map
{
    public interface IWorldMapManager
    {
        MapDataItem GetCell(Vector2I cell);
        bool ContainsCell(Vector2I cell);
        bool TryGetCell(Vector2I cell, out MapDataItem item);
        int CellCount { get; }
        Vector3I CellSize { get; set; }
        
        StaticBody3D RiverCollider { get; set; }
        CustomAStarPathfinder Pathfinder { get; }
        Node3D Terrain { get; set; }

        Vector3 CellToWorld(Vector2I cell, float height=0, bool centered = false);
        Vector3 CellToWorldInterpolated(Vector2 cell, float height = 0);
        Vector2I WorldToCell(Vector3 worldPos);
    }
}