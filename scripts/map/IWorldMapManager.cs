using Godot;
using SacaSimulationGame.scripts.managers;
using SacaSimulationGame.scripts.pathfinding;

namespace SacaSimulationGame.scripts.map
{
    public interface IWorldMapManager
    {
        int CellCount { get; }
        Vector3I CellSize { get; set; }
        GameManager GameManager { get; }
        int MapHeight { get; }
        int MapWidth { get; }
        int MaxX { get; }
        int MaxY { get; }
        int MinX { get; }
        int MinY { get; }
        AstarPathfinder Pathfinder { get; }
        Node3D Terrain { get; set; }
        TerrainMapper TerrainMapper { get; set; }

        bool CellInsideBounds(Vector2I cell);
        Vector3 CellToWorld(Vector2I cell, float height = 0, bool centered = false);
        Vector3 CellToWorldInterpolated(Vector2 cell, float height = 0);
        MapDataItem GetCell(Vector2I cell);
        bool TryGetCell(Vector2I cell, out MapDataItem item);
        bool WorldPosInsideBounds(Vector3 worldPos);
        Vector2I WorldToCell(Vector3 worldPos);
    }
}