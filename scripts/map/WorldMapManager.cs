using GameTemplate.scripts.map;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class WorldMapManager : Node3D
{
    [Export]
    public Vector2I CellSize { get; set; } = new Vector2I(3, 3);

    [Export]
    public Node3D Terrain { get; set; }

    [Export]
    public StaticBody3D RiverCollider { get; set; }


    [Export]
    public bool ShowSlopeGradients
    {
        get => _showSlopeGradients;
        set
        {
            _showSlopeGradients = value;
            if (GradientVisualizer != null)
            {
                GradientVisualizer.ShowSlopeGradients = value;
            }
        }
    }
    private bool _showSlopeGradients = true;

    private TerrainGradientVisualizer GradientVisualizer;
    public TerrainMapper TerrainMapper;
    public Dictionary<Vector2I, MapDataItem> MapData;


    public Vector2 CellToWorld(Vector2I cell)
    {
        var origin = new Vector2(Terrain.GlobalPosition.X, Terrain.GlobalPosition.Z);
        return origin + cell * CellSize;
    }

    public Vector2I WorldToCell(Vector3 worldPos)
    {
        var origin = new Vector2(Terrain.GlobalPosition.X, Terrain.GlobalPosition.Z);

        var worldPos2d = new Vector2(worldPos.X, worldPos.Z);
        var relativePos = worldPos2d - origin;

        var cellX = Mathf.FloorToInt(relativePos.X/CellSize.X);
        var cellY = Mathf.FloorToInt(relativePos.Y/CellSize.Y);
        return new Vector2I(cellX, cellY);

    }

    public override void _Ready()
    {
        GradientVisualizer = GetNode<TerrainGradientVisualizer>("TerrainGradientVisualizer");
        TerrainMapper = GetNode<TerrainMapper>("TerrainMapper");

        MapData = TerrainMapper.LoadMapdata(Terrain, CellSize);

        // Initialize gradient
        GradientVisualizer.Position = new Vector3(
            Terrain.GlobalTransform.Origin.X,
            GradientVisualizer.GlobalPosition.Y,
            Terrain.GlobalTransform.Origin.Z
        );

        GradientVisualizer.SetGradients(MapData, CellSize);
        GradientVisualizer.ShowSlopeGradients = ShowSlopeGradients;
    }
}