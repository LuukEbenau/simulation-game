using System.Collections.Generic;
using Godot;
using SacaSimulationGame.scripts.pathfinding;

namespace SacaSimulationGame.scripts.map
{
    public interface IWorldMapManager
    {
        Dictionary<Vector2I, MapDataItem> MapData { get; }

        Vector3I CellSize { get; set; }
        
        StaticBody3D RiverCollider { get; set; }
        AstarPathfinder Pathfinder { get; }
        Node3D Terrain { get; set; }

        Vector3 CellToWorld(Vector2I cell, float height=0, bool centered = false);
        Vector3 CellToWorldInterpolated(Vector2 cell, float height = 0);
        Vector2I WorldToCell(Vector3 worldPos);
    }
}