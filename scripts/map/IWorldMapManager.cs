using System.Collections.Generic;
using Godot;

namespace SacaSimulationGame.scripts.map
{
    public interface IWorldMapManager
    {
        Vector2I CellSize { get; set; }
        bool MapPropertiesCacheEnabled { get; set; }
        StaticBody3D RiverCollider { get; set; }
        bool ShowSlopeGradients { get; set; }
        Node3D Terrain { get; set; }

        Vector2 CellToWorld(Vector2I cell, bool centered = false);
        Vector2I WorldToCell(Vector3 worldPos);
        Dictionary<Vector2I, MapDataItem> MapData { get; }
        void _Ready();
    }
}